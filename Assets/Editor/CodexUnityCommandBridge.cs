#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace GetBricked.EditorTools
{
    [InitializeOnLoad]
    internal static class CodexUnityCommandBridge
    {
        private const double HeartbeatIntervalSeconds = 2.0d;
        private static readonly Regex CompilerErrorPattern = new Regex(
            @"error CS\d+|Scripts have compiler errors|Script Compilation Error|\*\*\* Tundra build failed",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly string BridgeRoot = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "CodexUnityBridge");
        private static readonly string RequestDirectory = Path.Combine(BridgeRoot, "requests");
        private static readonly string ResponseDirectory = Path.Combine(BridgeRoot, "responses");
        private static readonly string InflightDirectory = Path.Combine(BridgeRoot, "inflight");
        private static readonly string HeartbeatPath = Path.Combine(BridgeRoot, "editor-heartbeat.json");

        private static CodexBridgeRequest activeCompileRequest;
        private static double compileStartedAt;
        private static double nextHeartbeatAt;
        private static readonly List<string> compileErrors = new List<string>();

        static CodexUnityCommandBridge()
        {
            Directory.CreateDirectory(RequestDirectory);
            Directory.CreateDirectory(ResponseDirectory);
            Directory.CreateDirectory(InflightDirectory);
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            WriteHeartbeatIfDue();

            if (activeCompileRequest != null)
            {
                CompleteCompileWhenReady();
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }

            var requestPath = Directory.GetFiles(RequestDirectory, "*.json")
                .OrderBy(File.GetCreationTimeUtc)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(requestPath))
            {
                return;
            }

            ProcessRequest(requestPath);
        }

        private static void ProcessRequest(string requestPath)
        {
            CodexBridgeRequest request;
            try
            {
                request = JsonUtility.FromJson<CodexBridgeRequest>(File.ReadAllText(requestPath));
                if (request == null || string.IsNullOrWhiteSpace(request.id) || string.IsNullOrWhiteSpace(request.command))
                {
                    File.Delete(requestPath);
                    return;
                }

                var inflightPath = Path.Combine(InflightDirectory, Path.GetFileName(requestPath));
                if (File.Exists(inflightPath))
                {
                    File.Delete(inflightPath);
                }

                File.Move(requestPath, inflightPath);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Codex bridge could not read request {requestPath}: {exception}");
                return;
            }

            try
            {
                switch (request.command.ToLowerInvariant())
                {
                    case "compile":
                        StartCompileCheck(request);
                        break;
                    case "tests":
                        StartTests(request);
                        break;
                    default:
                        WriteResponse(request, false, "unknown-command", $"Unknown Codex bridge command '{request.command}'.");
                        break;
                }
            }
            catch (Exception exception)
            {
                WriteResponse(request, false, "failed", exception.ToString());
            }
        }

        private static void StartCompileCheck(CodexBridgeRequest request)
        {
            activeCompileRequest = request;
            compileStartedAt = EditorApplication.timeSinceStartup;
            compileErrors.Clear();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void CompleteCompileWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup - compileStartedAt < 0.5d)
            {
                return;
            }

            var errors = compileErrors.Concat(ReadCompilerErrorsFromConsole()).Distinct().ToArray();
            if (errors.Length == 0)
            {
                WriteResponse(activeCompileRequest, true, "passed", "Unity editor compile check passed.");
            }
            else
            {
                WriteResponse(activeCompileRequest, false, "failed", "Unity editor compile check failed.", errors);
            }

            activeCompileRequest = null;
            compileErrors.Clear();
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (activeCompileRequest == null || messages == null)
            {
                return;
            }

            foreach (var message in messages)
            {
                if (message.type == CompilerMessageType.Error)
                {
                    compileErrors.Add(message.message);
                }
            }
        }

        private static void StartTests(CodexBridgeRequest request)
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var callbacks = ScriptableObject.CreateInstance<CodexTestCallbacks>();
            callbacks.Initialize(request);
            api.RegisterCallbacks(callbacks);

            var filter = new Filter
            {
                testMode = string.Equals(request.platform, "playmode", StringComparison.OrdinalIgnoreCase)
                    ? UnityEditor.TestTools.TestRunner.Api.TestMode.PlayMode
                    : UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode
            };

            if (!string.IsNullOrWhiteSpace(request.testFilter))
            {
                filter.groupNames = new[] { request.testFilter };
            }

            api.Execute(new ExecutionSettings(filter));
        }

        private static IEnumerable<string> ReadCompilerErrorsFromConsole()
        {
            var entriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            var entryType = Type.GetType("UnityEditor.LogEntry,UnityEditor.dll");
            if (entriesType == null || entryType == null)
            {
                yield break;
            }

            var getCount = entriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var getEntry = entriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var conditionField = entryType.GetField("condition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (getCount == null || getEntry == null || conditionField == null)
            {
                yield break;
            }

            var count = (int)getCount.Invoke(null, null);
            for (var i = 0; i < count; i++)
            {
                var entry = Activator.CreateInstance(entryType);
                getEntry.Invoke(null, new[] { (object)i, entry });
                var condition = conditionField.GetValue(entry) as string;
                if (!string.IsNullOrWhiteSpace(condition) && CompilerErrorPattern.IsMatch(condition))
                {
                    yield return condition.Trim();
                }
            }
        }

        private static void WriteHeartbeatIfDue()
        {
            if (EditorApplication.timeSinceStartup < nextHeartbeatAt)
            {
                return;
            }

            nextHeartbeatAt = EditorApplication.timeSinceStartup + HeartbeatIntervalSeconds;
            Directory.CreateDirectory(BridgeRoot);
            File.WriteAllText(
                HeartbeatPath,
                BuildJson(
                    ("status", "ready"),
                    ("timeUtc", DateTime.UtcNow.ToString("O")),
                    ("unityVersion", Application.unityVersion)));
        }

        private static void WriteResponse(CodexBridgeRequest request, bool success, string status, string message, IEnumerable<string> errors = null)
        {
            Directory.CreateDirectory(ResponseDirectory);
            var responsePath = Path.Combine(ResponseDirectory, request.id + ".json");
            File.WriteAllText(
                responsePath,
                BuildJson(
                    ("id", request.id),
                    ("command", request.command),
                    ("platform", request.platform),
                    ("success", success ? "true" : "false"),
                    ("status", status),
                    ("message", message),
                    ("resultsPath", request.resultsPath),
                    ("errors", errors == null ? string.Empty : string.Join("\n", errors))));
        }

        private static string BuildJson(params (string key, string value)[] values)
        {
            var builder = new StringBuilder();
            builder.Append('{');
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }

                builder.Append('"').Append(EscapeJson(values[i].key)).Append("\":");
                if (values[i].value == "true" || values[i].value == "false")
                {
                    builder.Append(values[i].value);
                }
                else
                {
                    builder.Append('"').Append(EscapeJson(values[i].value ?? string.Empty)).Append('"');
                }
            }

            builder.Append('}');
            return builder.ToString();
        }

        private static string EscapeJson(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        [Serializable]
        private sealed class CodexBridgeRequest
        {
            public string id;
            public string command;
            public string platform;
            public string testFilter;
            public string resultsPath;
        }

        private sealed class CodexTestCallbacks : ScriptableObject, ICallbacks
        {
            private CodexBridgeRequest request;

            public void Initialize(CodexBridgeRequest bridgeRequest)
            {
                request = bridgeRequest;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(request.resultsPath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(request.resultsPath));
                        TestRunnerApi.SaveResultToFile(result, request.resultsPath);
                    }

                    var success = !result.ResultState.StartsWith("Failed", StringComparison.OrdinalIgnoreCase)
                        && result.FailCount == 0;
                    WriteResponse(
                        request,
                        success,
                        success ? "passed" : "failed",
                        $"Unity editor test run finished: total={TotalCount(result)} passed={result.PassCount} failed={result.FailCount} skipped={result.SkipCount} inconclusive={result.InconclusiveCount}.");
                }
                catch (Exception exception)
                {
                    WriteResponse(request, false, "failed", exception.ToString());
                }
                finally
                {
                    TestRunnerApi.UnregisterTestCallback(this);
                    DestroyImmediate(this);
                }
            }

            private static int TotalCount(ITestResultAdaptor result)
            {
                return result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount;
            }
        }
    }
}
#endif
