using Microsoft.MixedReality.Toolkit.Core.Utilities.Editor;
using Microsoft.MixedReality.Toolkit.Core.Utilities.WindowsDevicePortal.DataStructures;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Utilities.Build
{
    public static class UwpBuildDeployPreferences
    {
        private const string EDITOR_PREF_BUILD_CONFIG = "BuildDeployWindow_BuildConfig";
        private const string EDITOR_PREF_BUILD_PLATFORM = "BuildDeployWindow_BuildPlatform";
        private const string EDITOR_PREF_FORCE_REBUILD = "BuildDeployWindow_ForceRebuild";
        private const string EDITOR_PREF_CONNECT_INFOS = "BuildDeployWindow_DeviceConnections";
        private const string EDITOR_PREF_FULL_REINSTALL = "BuildDeployWindow_FullReinstall";
        private const string EDITOR_PREF_USE_SSL = "BuildDeployWindow_UseSSL";
        private const string EDITOR_PREF_PROCESS_ALL = "BuildDeployWindow_ProcessAll";

        /// <summary>
        /// The current Build Configuration. (Debug, Release, or Master)
        /// </summary>
        public static string BuildConfig
        {
            get => EditorPreferences.Get(EDITOR_PREF_BUILD_CONFIG, "Debug");
            set => EditorPreferences.Set(EDITOR_PREF_BUILD_CONFIG, value);
        }

        /// <summary>
        /// The current Build Platform. (x86 or x64)
        /// </summary>
        public static string BuildPlatform
        {
            get => EditorPreferences.Get(EDITOR_PREF_BUILD_PLATFORM, "x86");
            set => EditorPreferences.Set(EDITOR_PREF_BUILD_PLATFORM, value);
        }

        /// <summary>
        /// Current setting to force rebuilding the appx.
        /// </summary>
        public static bool ForceRebuild
        {
            get => EditorPreferences.Get(EDITOR_PREF_FORCE_REBUILD, false);
            set => EditorPreferences.Set(EDITOR_PREF_FORCE_REBUILD, value);
        }

        /// <summary>
        /// Current setting to fully uninstall and reinstall the appx.
        /// </summary>
        public static bool FullReinstall
        {
            get => EditorPreferences.Get(EDITOR_PREF_FULL_REINSTALL, true);
            set => EditorPreferences.Set(EDITOR_PREF_FULL_REINSTALL, value);
        }

        /// <summary>
        /// The current device portal connections.
        /// </summary>
        public static string DevicePortalConnections
        {
            get => EditorPreferences.Get(
                    EDITOR_PREF_CONNECT_INFOS,
                    JsonUtility.ToJson(
                            new DevicePortalConnections(
                                    new DeviceInfo("127.0.0.1", string.Empty, string.Empty, "Local Machine"))));
            set => EditorPreferences.Set(EDITOR_PREF_CONNECT_INFOS, value);
        }

        /// <summary>
        /// Current setting to use Single Socket Layer connections to the device portal.
        /// </summary>
        public static bool UseSSL
        {
            get => EditorPreferences.Get(EDITOR_PREF_USE_SSL, true);
            set => EditorPreferences.Set(EDITOR_PREF_USE_SSL, value);
        }

        /// <summary>
        /// Current setting to target all the devices registered to the build window.
        /// </summary>
        public static bool TargetAllConnections
        {
            get => EditorPreferences.Get(EDITOR_PREF_PROCESS_ALL, false);
            set => EditorPreferences.Set(EDITOR_PREF_PROCESS_ALL, value);
        }
    }
}