using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Core.Extensions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Microsoft.MixedReality.Toolkit.Core.Utilities.Build
{
    /// <summary>
    /// Cross platform player build tools
    /// </summary>
    public static class UnityPlayerBuildTools
    {
        // Build configurations. Exactly one of these should be defined for any given build.
        public const string BuildSymbolDebug = "debug";
        public const string BuildSymbolRelease = "release";
        public const string BuildSymbolMaster = "master";

        /// <summary>
        /// Starts the build process
        /// </summary>
        /// <param name="buildInfo"></param>
        /// <returns>The <see cref="BuildReport"/> from Unity's <see cref="BuildPipeline"/></returns>
        public static BuildReport BuildUnityPlayer(IBuildInfo buildInfo)
        {
            EditorUtility.DisplayProgressBar("Build Pipeline", "Gathering Build Data...", 0.25f);

            // Call the pre-build action, if any
            buildInfo.PreBuildAction?.Invoke(buildInfo);

            BuildTargetGroup buildTargetGroup = buildInfo.BuildTarget.GetGroup();
            string playerBuildSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            if (!string.IsNullOrEmpty(playerBuildSymbols))
            {
                if (buildInfo.HasConfigurationSymbol())
                {
                    buildInfo.AppendWithoutConfigurationSymbols(playerBuildSymbols);
                }
                else
                {
                    buildInfo.AppendSymbols(playerBuildSymbols.Split(';'));
                }
            }

            if (!string.IsNullOrEmpty(buildInfo.BuildSymbols))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, buildInfo.BuildSymbols);
            }

            if ((buildInfo.BuildOptions & BuildOptions.Development) == BuildOptions.Development)
            {
                if (!buildInfo.HasConfigurationSymbol())
                {
                    buildInfo.AppendSymbols(BuildSymbolDebug);
                }
            }

            if (buildInfo.HasAnySymbols(BuildSymbolDebug))
            {
                buildInfo.BuildOptions |= BuildOptions.Development | BuildOptions.AllowDebugging;
            }

            if (buildInfo.HasAnySymbols(BuildSymbolRelease))
            {
                // Unity automatically adds the DEBUG symbol if the BuildOptions.Development flag is
                // specified. In order to have debug symbols and the RELEASE symbols we have to
                // inject the symbol Unity relies on to enable the /debug+ flag of csc.exe which is "DEVELOPMENT_BUILD"
                buildInfo.AppendSymbols("DEVELOPMENT_BUILD");
            }

            var oldColorSpace = PlayerSettings.colorSpace;

            if (buildInfo.ColorSpace.HasValue)
            {
                PlayerSettings.colorSpace = buildInfo.ColorSpace.Value;
            }

            BuildTarget oldBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup oldBuildTargetGroup = oldBuildTarget.GetGroup();

            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildInfo.BuildTarget);
            Directory.CreateDirectory(buildInfo.OutputDirectory);

            BuildReport buildReport = default;

            try
            {
                buildReport = BuildPipeline.BuildPlayer(
                        buildInfo.Scenes.ToArray(),
                        buildInfo.OutputDirectory,
                        buildInfo.BuildTarget,
                        buildInfo.BuildOptions);
            }
            catch
            {
                // ignored bc unity will catch it for us and write it to the console.
            }

            PlayerSettings.colorSpace = oldColorSpace;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, playerBuildSymbols);
            EditorUserBuildSettings.SwitchActiveBuildTarget(oldBuildTargetGroup, oldBuildTarget);

            // Call the post-build action, if any
            buildInfo.PostBuildAction?.Invoke(buildInfo, buildReport);

            return buildReport;
        }

        /// <summary>
        /// Start a build using Unity's command line.
        /// </summary>
        public static async void StartCommandLineBuild()
        {
            // We don't need stack traces on all our logs. Makes things a lot easier to read.
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            bool success;

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.WSAPlayer:
                    success = await UwpPlayerBuildTools.CommandLine_BuildUwpPlayer();
                    break;
                default:
                    var buildInfo = new BuildInfo(true) as IBuildInfo;
                    ParseBuildCommandLine(ref buildInfo);
                    var buildResult = BuildUnityPlayer(buildInfo);
                    success = buildResult.summary.result == BuildResult.Succeeded;
                    break;
            }

            EditorApplication.Exit(success ? 0 : 1);
        }

        internal static bool CheckBuildScenes()
        {
            if (EditorBuildSettings.scenes.Length == 0)
            {
                return EditorUtility.DisplayDialog("Attention!",
                                                   "No scenes are present in the build settings.\n" +
                                                   "The current scene will be the one built.\n\n" +
                                                   "Do you want to cancel and add one?",
                                                   "Continue Anyway", "Cancel Build");
            }

            return true;
        }

        /// <summary>
        /// Get the Unity Project Root Path.
        /// </summary>
        /// <returns>The full path to the project's root.</returns>
        public static string GetProjectPath()
        {
            return Path.GetDirectoryName(Path.GetFullPath(Application.dataPath));
        }

        public static void ParseBuildCommandLine(ref IBuildInfo buildInfo)
        {
            string[] arguments = Environment.GetCommandLineArgs();

            for (int i = 0; i < arguments.Length; ++i)
            {
                switch (arguments[i])
                {
                    case "-autoIncrement":
                        buildInfo.AutoIncrement = true;
                        break;
                    case "-scenes":
                        // TODO parse json scene list and set them.
                        break;
                    case "-buildOutput":
                        buildInfo.OutputDirectory = arguments[++i];
                        break;
                    case "-colorSpace":
                        buildInfo.ColorSpace = (ColorSpace)Enum.Parse(typeof(ColorSpace), arguments[++i]);
                        break;
                    case "-x86":
                    case "-x64":
                        buildInfo.BuildPlatform = arguments[i].Substring(1);
                        break;
                    case "-debug":
                    case "-master":
                    case "-release":
                        buildInfo.Configuration = arguments[i].Substring(1);
                        break;
                }
            }
        }

        /// <summary>
        /// Restores any nuget packages at the path specified.
        /// </summary>
        /// <param name="nugetPath"></param>
        /// <param name="storePath"></param>
        /// <returns>True, if the nuget packages were successfully restored.</returns>
        public static async Task<bool> RestoreNugetPackagesAsync(string nugetPath, string storePath)
        {
            Debug.Assert(File.Exists(nugetPath));
            Debug.Assert(Directory.Exists(storePath));

            await new Process().StartProcessAsync(nugetPath, $"restore \"{storePath}/project.json\"");

            return File.Exists($"{storePath}\\project.lock.json");
        }
    }
}