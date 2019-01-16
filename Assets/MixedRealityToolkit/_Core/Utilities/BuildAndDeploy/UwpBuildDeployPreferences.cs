using Microsoft.MixedReality.Toolkit.Core.Utilities.Editor;
using Microsoft.MixedReality.Toolkit.Core.Utilities.WindowsDevicePortal.DataStructures;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Utilities.Build
{
    public static class UwpBuildDeployPreferences
    {
        public const string MIN_SDK_VERSION = "10.0.17134.0";
        private const string EDITOR_PREF_BUILD_CONFIG = "BuildDeployWindow_BuildConfig";
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
        private const string EDITOR_PREF_3D_APP_ICON = "BuildDeployWindow_3D_AppIcon";

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
        /// The path to the 3d app icon .glb asset.
        /// </summary>
        public static string _3DAppIconPath
        {
            get => EditorPreferences.Get(EDITOR_PREF_3D_APP_ICON, "");
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Debug.Assert(value.EndsWith(".glb"), "3d App Icon must be a .glb asset");
                }

                EditorPreferences.Set(EDITOR_PREF_3D_APP_ICON, value);
            }
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