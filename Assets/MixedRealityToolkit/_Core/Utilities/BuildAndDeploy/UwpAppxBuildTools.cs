// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Extensions;
using Microsoft.MixedReality.Toolkit.Core.Utilities.Editor;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Microsoft.MixedReality.Toolkit.Core.Utilities.Build
{
    public class UwpAppxBuildTools
    {
        /// <summary>
        /// Query the build process to see if we're already building.
        /// </summary>
        public static bool IsBuilding { get; private set; } = false;

        /// <summary>
        /// Build the UWP appx bundle for this project.  Requires that <see cref="UwpPlayerBuildTools.BuildPlayer(string,bool,bool)"/> has already be run or a user has
        /// previously built the Unity Player with the WSA Player as the Build Target.
        /// </summary>
        /// <param name="productName">The applications product name. Typically <see cref="PlayerSettings.productName"/></param>
        /// <param name="forceRebuildAppx">Should we force rebuild the appx bundle?</param>
        /// <param name="buildConfig">Debug, Release, or Master configurations are valid.</param>
        /// <param name="buildPlatform">x86 or x64 build platforms are valid.</param>
        /// <param name="buildDirectory">The directory where the built Unity Player Solution is located.</param>
        /// <param name="incrementVersion">Should we increment the appx version number?</param>
        /// <returns>True, if the appx build was successful.</returns>
        public static async Task<bool> BuildAppxAsync(string productName, bool forceRebuildAppx, string buildConfig, string buildPlatform, string buildDirectory, bool incrementVersion)
        {
            if (!EditorAssemblyReloadManager.LockReloadAssemblies)
            {
                Debug.LogError("Lock Reload assemblies before attempting to build appx!");
                return false;
            }

            if (IsBuilding)
            {
                Debug.LogWarning("Build already in progress!");
                return false;
            }

            if (Application.isBatchMode)
            {
                // We don't need stack traces on all our logs. Makes things a lot easier to read.
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            }

            Debug.Log("Starting Unity Appx Build...");

            IsBuilding = true;
            string slnFilename = Path.Combine(buildDirectory, $"{PlayerSettings.productName}.sln");

            if (!File.Exists(slnFilename))
            {
                Debug.LogError("Unable to find Solution to build from!");
                return IsBuilding = false;
            }

            // Get and validate the msBuild path...
            var msBuildPath = await FindMsBuildPathAsync();

            if (!File.Exists(msBuildPath))
            {
                Debug.LogError($"MSBuild.exe is missing or invalid!\n{msBuildPath}");
                return IsBuilding = false;
            }

            // Ensure that the generated .appx version increments by modifying Package.appxmanifest
            if (!SetPackageVersion(incrementVersion))
            {
                Debug.LogError("Failed to increment package version!");
                return IsBuilding = false;
            }

            string storagePath = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, ".."), buildDirectory));
            string solutionProjectPath = Path.GetFullPath(Path.Combine(storagePath, $@"{productName}.sln"));

            // Now do the actual appx build
            var processResult = await new Process().StartProcessAsync(
                msBuildPath,
                $"\"{solutionProjectPath}\" /t:{(forceRebuildAppx ? "Rebuild" : "Build")} /p:Configuration={buildConfig} /p:Platform={buildPlatform} /verbosity:m",
                !Application.isBatchMode);

            switch (processResult.ExitCode)
            {
                case 0:
                    Debug.Log("Appx Build Successful!");

                    if (Application.isBatchMode)
                    {
                        Debug.Log(string.Join("\n", processResult.Output));
                    }
                    break;
                case -1073741510:
                    Debug.LogWarning("The build was terminated either by user's keyboard input CTRL+C or CTRL+Break or closing command prompt window.");
                    break;
                default:
                    {
                        if (processResult.ExitCode != 0)
                        {
                            Debug.LogError($"{PlayerSettings.productName} appx build Failed! (ErrorCode: {processResult.ExitCode})");

                            if (Application.isBatchMode)
                            {
                                Debug.LogError(string.Join("\n", processResult.Errors));
                            }
                        }

                        break;
                    }
            }

            AssetDatabase.SaveAssets();

            IsBuilding = false;
            return processResult.ExitCode == 0;
        }

        private static async Task<string> FindMsBuildPathAsync()
        {
            var result = await new Process().StartProcessAsync(
                new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = $@"/C vswhere -all -products * -requires Microsoft.Component.MSBuild -property installationPath",
                    WorkingDirectory = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer"
                });

            foreach (var path in result.Output)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    string[] paths = path.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    if (paths.Length > 0)
                    {
                        // if there are multiple visual studio installs,
                        // prefer enterprise, then pro, then community
                        string bestPath = paths.OrderBy(p => p.ToLower().Contains("enterprise"))
                            .ThenBy(p => p.ToLower().Contains("professional"))
                            .ThenBy(p => p.ToLower().Contains("community")).First();

                        return $@"{bestPath}\MSBuild\15.0\Bin\MSBuild.exe";
                    }
                }
            }

            return string.Empty;
        }

        private static bool SetPackageVersion(bool increment)
        {
            // Find the manifest, assume the one we want is the first one
            string[] manifests = Directory.GetFiles(BuildDeployPreferences.AbsoluteBuildDirectory, "Package.appxmanifest", SearchOption.AllDirectories);

            if (manifests.Length == 0)
            {
                Debug.LogError($"Unable to find Package.appxmanifest file for build (in path - {BuildDeployPreferences.AbsoluteBuildDirectory})");
                return false;
            }

            string manifest = manifests[0];
            var rootNode = XElement.Load(manifest);
            var identityNode = rootNode.Element(rootNode.GetDefaultNamespace() + "Identity");

            if (identityNode == null)
            {
                Debug.LogError($"Package.appxmanifest for build (in path - {BuildDeployPreferences.AbsoluteBuildDirectory}) is missing an <Identity /> node");
                return false;
            }

            // We use XName.Get instead of string -> XName implicit conversion because
            // when we pass in the string "Version", the program doesn't find the attribute.
            // Best guess as to why this happens is that implicit string conversion doesn't set the namespace to empty
            var versionAttr = identityNode.Attribute(XName.Get("Version"));

            if (versionAttr == null)
            {
                Debug.LogError($"Package.appxmanifest for build (in path - {BuildDeployPreferences.AbsoluteBuildDirectory}) is missing a version attribute in the <Identity /> node.");
                return false;
            }

            // Assume package version always has a '.' between each number.
            // According to https://msdn.microsoft.com/en-us/library/windows/apps/br211441.aspx
            // Package versions are always of the form Major.Minor.Build.Revision.
            // Note: Revision number reserved for Windows Store, and a value other than 0 will fail WACK.
            var version = PlayerSettings.WSA.packageVersion;
            var newVersion = new Version(version.Major, version.Minor, increment ? version.Build + 1 : version.Build, version.Revision);

            PlayerSettings.WSA.packageVersion = newVersion;
            versionAttr.Value = newVersion.ToString();
            rootNode.Save(manifest);
            return true;
        }
    }
}