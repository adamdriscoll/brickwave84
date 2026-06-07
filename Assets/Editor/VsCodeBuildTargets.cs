#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

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
