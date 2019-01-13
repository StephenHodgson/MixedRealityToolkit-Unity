// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Utilities.Editor;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Utilities.Build
{
    /// <summary>
    /// Class containing various utility methods to build a WSA solution from a Unity project.
    /// </summary>
    public static class UwpPlayerBuildTools
    {
        private static void ParseBuildCommandLine(ref UwpBuildInfo buildInfo)
        {
            IBuildInfo iBuildInfo = buildInfo;
            UnityPlayerBuildTools.ParseBuildCommandLine(ref iBuildInfo);

            string[] arguments = Environment.GetCommandLineArgs();

            for (int i = 0; i < arguments.Length; ++i)
            {
                switch (arguments[i])
                {
                    case "-buildAppx":
                        buildInfo.BuildAppx = true;
                        break;
                    case "-rebuildAppx":
                        buildInfo.RebuildAppx = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Do a build configured for UWP Applications to the specified path, returns the error from <see cref="BuildPlayer(UwpBuildInfo)"/>
        /// </summary>
        /// <param name="buildDirectory"></param>
        /// <param name="showDialog"></param>
        /// <param name="buildAppx"></param>
        /// <returns>True, if build was successful.</returns>
        public static async Task<bool> BuildPlayer(string buildDirectory, bool showDialog = true, bool buildAppx = false)
        {
            if (UnityPlayerBuildTools.CheckBuildScenes() == false)
            {
                return false;
            }

            var buildInfo = new UwpBuildInfo
            {
                OutputDirectory = buildDirectory,
                Scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path),
                BuildAppx = buildAppx,

                // Configure a post build action that will compile the generated solution
                PostBuildAction = PostBuildAction
            };

            async void PostBuildAction(IBuildInfo innerBuildInfo, BuildReport buildReport)
            {
                if (buildReport.summary.result != BuildResult.Succeeded)
                {
                    EditorUtility.DisplayDialog($"{PlayerSettings.productName} WindowsStoreApp Build {buildReport.summary.result}!", "See console for details", "OK");
                }
                else
                {
                    if (showDialog &&
                        !EditorUtility.DisplayDialog(PlayerSettings.productName, "Build Complete", "OK", "Build AppX"))
                    {
                        var _buildInfo = innerBuildInfo as UwpBuildInfo;
                        Debug.Assert(_buildInfo != null);
                        EditorAssemblyReloadManager.LockReloadAssemblies = true;
                        await UwpAppxBuildTools.BuildAppxAsync(
                                PlayerSettings.productName,
                                _buildInfo.RebuildAppx,
                                _buildInfo.Configuration,
                                _buildInfo.BuildPlatform,
                                _buildInfo.OutputDirectory,
                                _buildInfo.AutoIncrement);
                        EditorAssemblyReloadManager.LockReloadAssemblies = false;
                    }
                }
            }

            return await BuildPlayer(buildInfo);
        }

        /// <summary>
        /// Build the Uwp Player.
        /// </summary>
        /// <param name="buildInfo"></param>
        public static async Task<bool> BuildPlayer(UwpBuildInfo buildInfo)
        {
            #region Gather Build Data

            if (buildInfo.IsCommandLine)
            {
                ParseBuildCommandLine(ref buildInfo);
            }

            #endregion Gather Build Data

            BuildReport buildReport = UnityPlayerBuildTools.BuildUnityPlayer(buildInfo);

            bool success = buildReport != null && buildReport.summary.result == BuildResult.Succeeded;

            if (success && buildInfo.BuildAppx)
            {
                success &= await UwpAppxBuildTools.BuildAppxAsync(
                    PlayerSettings.productName,
                    buildInfo.RebuildAppx,
                    buildInfo.Configuration,
                    buildInfo.BuildPlatform,
                    buildInfo.OutputDirectory,
                    buildInfo.AutoIncrement);
            }
            
            return success;
        }
    }
}