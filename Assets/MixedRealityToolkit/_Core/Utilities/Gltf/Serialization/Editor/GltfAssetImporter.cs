using UnityEditor.Experimental.AssetImporters;

namespace Microsoft.MixedReality.Toolkit.Core.Utilities.Gltf.Serialization.Editor
{
    [ScriptedImporter(1, "gltf")]
    public class GltfAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            GltfEditorImporter.OnImportGltfAsset(context);
        }
    }
}