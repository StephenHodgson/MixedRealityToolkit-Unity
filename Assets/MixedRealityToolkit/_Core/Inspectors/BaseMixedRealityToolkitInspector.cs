using Microsoft.MixedReality.Toolkit.Core.Inspectors.Utilities;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Inspectors.Profiles {
    /// <summary>
    /// Base class for all editor inspectors of the Mixed Reality Toolkit to inherit from.
    /// </summary>
    public abstract class BaseMixedRealityToolkitInspector : Editor
    {
        /// <summary>
        /// Render the Mixed Reality Toolkit Logo.
        /// </summary>
        protected static void RenderMixedRealityToolkitLogo()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(EditorGUIUtility.isProSkin ? MixedRealityInspectorUtility.LightThemeLogo : MixedRealityInspectorUtility.DarkThemeLogo, GUILayout.MaxHeight(128f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }
    }
}