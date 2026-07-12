#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GetBricked.EditorTools
{
    public static class VsCodeBuildTargets
    {
        private const string DefaultBuildPath = "Builds/Codex/Brickwave84.exe";

        public static void BuildWindowsPlayer()
        {
            BuildWindowsPlayer(BuildOptions.None);
        }

        public static void BuildWindowsDevelopmentPlayer()
        {
            BuildWindowsPlayer(BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        private static void BuildWindowsPlayer(BuildOptions options)
        {
            ApplyInstallerVersion();

            var outputPath = ResolveOutputPath();
            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new BuildFailedException($"Invalid VS Code build path: {outputPath}");
            }

            Directory.CreateDirectory(outputDirectory);

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new BuildFailedException("No enabled scenes are configured in Editor Build Settings.");
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = options
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"VS Code Windows player build failed: {report.summary.result} ({report.summary.totalErrors} errors).");
            }

            UnityEngine.Debug.Log($"VS Code Windows player build succeeded: {outputPath}");
        }

        private static void ApplyInstallerVersion()
        {
            var versionPath = Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(),
                "Installer",
                "ProductVersion.props"));

            if (!File.Exists(versionPath))
            {
                return;
            }

            try
            {
                var document = XDocument.Load(versionPath);
                var version = document.Descendants("ProductVersion").FirstOrDefault()?.Value?.Trim();
                if (string.IsNullOrWhiteSpace(version) || !Version.TryParse(version, out _))
                {
                    Debug.LogWarning($"Installer version file does not contain a valid ProductVersion: {versionPath}");
                    return;
                }

                PlayerSettings.bundleVersion = version;
                Debug.Log($"Applied Brickwave installer version to player build: {version}");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not read installer version from {versionPath}: {exception.Message}");
            }
        }

        private static string ResolveOutputPath()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], "-vscodeBuildPath", StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetFullPath(args[i + 1]);
                }
            }

            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), DefaultBuildPath));
        }
    }
}
#endif
