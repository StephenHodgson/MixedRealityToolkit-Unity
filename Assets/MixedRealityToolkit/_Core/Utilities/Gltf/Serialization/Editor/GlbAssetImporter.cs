using UnityEditor.Experimental.AssetImporters;

namespace Microsoft.MixedReality.Toolkit.Core.Utilities.Gltf.Serialization.Editor
{
    [ScriptedImporter(1, "glb")]
    public class GlbAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            GltfEditorImporter.OnImportGltfAsset(context);
        }
    }

    public class GlbModelAssetImporterEditor : AssetImporterEditor
    {

    }
}