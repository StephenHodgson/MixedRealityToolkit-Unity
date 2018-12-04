// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using UnityAssembly = UnityEditor.Compilation.Assembly;
using UnityObject = UnityEngine.Object;

namespace Microsoft.MixedReality.Toolkit.Core.Inspectors.Utilities
{
    public static class AssemblyDefinitionEditorExtension
    {
        private const string DLL = ".dll";
        private const string META = ".meta";
        private const string JSON = ".json";
        private const string ASMDEF = ".asmdef";
        private const string DEFAULT_SCRIPT_FILE_ID = "11500000";

        private static readonly string[] DefaultFileExtensions =
        {
            "*.meta",
            "*.mat",
            "*.anim",
            "*.prefab",
            "*.unity",
            "*.asset",
            "*.guiskin",
            "*.fontsettings",
            "*.controller",
        };

        /// <summary>
        /// The script guid table from the processed source assemblies.
        /// </summary>
        /// <remarks>
        /// If going from assembly to source then the key, value is: &lt;scriptName, guid&gt; otherwise the table is reversed.
        /// </remarks>
        private static readonly Dictionary<string, string> ScriptGuidTable = new Dictionary<string, string>();

        /// <summary>
        /// The file id table from the processed scripts and dll.
        /// </summary>
        /// <remarks>
        /// If going from assembly to source then they key, value is: &lt;scriptFileId, scriptName&gt; otherwise the table is reversed.
        /// </remarks>
        private static readonly Dictionary<string, string> FileIdTable = new Dictionary<string, string>();

        #region Serialzied Data Objects

        [Serializable]
        internal class AsmDefSourceFiles
        {
            public SourceFile[] Files = null;
        }

        [Serializable]
        internal struct SourceFile
        {
            public string Path;
            public string Guid;
            public long FileId;

            public SourceFile(string path, string guid, long fileId)
            {
                Path = path;
                Guid = guid;
                FileId = fileId;
            }
        }

        [Serializable]
        internal class CustomScriptAssemblyData
        {
            public string name = null;
            public string[] references = null;
            public string[] optionalUnityReferences = null;
            public string[] includePlatforms = null;
            public string[] excludePlatforms = null;
            public bool allowUnsafeCode = false;

            public AsmDefSourceFiles Source { get; set; } = new AsmDefSourceFiles();

            public static CustomScriptAssemblyData FromJson(string json)
            {
                var scriptAssemblyData = JsonUtility.FromJson<CustomScriptAssemblyData>(json);
                if (scriptAssemblyData == null) { throw new Exception("Json file does not contain an assembly definition"); }
                if (string.IsNullOrEmpty(scriptAssemblyData.name)) { throw new Exception("Required property 'name' not set"); }
                if (scriptAssemblyData.excludePlatforms != null && scriptAssemblyData.excludePlatforms.Length > 0 &&
                   (scriptAssemblyData.includePlatforms != null && scriptAssemblyData.includePlatforms.Length > 0))
                {
                    throw new Exception("Both 'excludePlatforms' and 'includePlatforms' are set.");
                }

                return scriptAssemblyData;
            }

            public static string ToJson(CustomScriptAssemblyData data)
            {
                return JsonUtility.ToJson(data, true);
            }
        }

        #endregion Serialzied Data Objects

        /// <summary>
        /// Replace Source with Assembly Menu Item Validation.
        /// </summary>
        /// <returns>True, if menu item is active.</returns>
        [MenuItem("CONTEXT/AssemblyDefinitionImporter/Replace Source with Assembly", true, 99)]
        public static bool ReplaceWithAssemblyValidation()
        {
            if (Selection.activeObject == null) { return false; }

            Debug.Assert(EditorSettings.serializationMode == SerializationMode.ForceText, "Editor settings must use force text serialization for this process to work.");
            return !AssetDatabase.GetAssetPath(Selection.activeObject).GetAssetPathSiblings().Any(path => path.Contains(DLL));
        }

        [MenuItem("CONTEXT/AssemblyDefinitionImporter/Replace Source with Assembly", false, 99)]
        public static void ReplaceWithAssembly()
        {
            Debug.Assert(Selection.activeObject != null);

            EditorUtility.DisplayProgressBar("Replacing source with assembly", "Getting things ready...", 0);

            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var directoryPath = new FileInfo(assetPath).Directory?.FullName;
            var assemblyDefinitionText = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assetPath).text;
            var scriptAssemblyData = CustomScriptAssemblyData.FromJson(assemblyDefinitionText);
            var fromAssemblyName = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(scriptAssemblyData.name);

            Debug.Assert(!string.IsNullOrEmpty(scriptAssemblyData.name));
            Debug.Assert(fromAssemblyName == assetPath, "Failed to get the proper assembly name!");

            if (CompilationPipeline.GetAssemblies(AssembliesType.Editor).ReplaceSourceWithAssembly(ref scriptAssemblyData, directoryPath) ||
                CompilationPipeline.GetAssemblies(AssembliesType.Player).ReplaceSourceWithAssembly(ref scriptAssemblyData, directoryPath))
            {
                var asmdefFullPath = Path.GetFullPath(assetPath);
                var asmdefHiddenPath = asmdefFullPath.Hide();

                EditorUtility.DisplayProgressBar("Replacing source with assembly", "Saving source meta data for later...", 0.95f);
                File.WriteAllText($"{asmdefHiddenPath}{JSON}", JsonUtility.ToJson(scriptAssemblyData.Source, true));

                Debug.Log($"{asmdefFullPath}\n{asmdefHiddenPath}");

                File.Move(asmdefFullPath, asmdefHiddenPath);
                File.Move($"{asmdefFullPath}{META}", $"{asmdefHiddenPath}{META}");
            }
            else
            {
                Debug.LogError("Failed to replace source code with assembly!");
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            EditorUtility.ClearProgressBar();
        }

        private static bool ReplaceSourceWithAssembly(this UnityAssembly[] assemblies, ref CustomScriptAssemblyData assemblyData, string directoryPath)
        {
            EditorUtility.DisplayProgressBar("Replacing source with assembly", "Gathering assembly information...", 0.1f);

            for (var i = 0; i < assemblies.Length; i++)
            {
                UnityAssembly assembly = assemblies[i];
                EditorUtility.DisplayProgressBar("Replacing source with assembly", $"Processing assembly {assembly.name}", i / (float)assemblies.Length);

                if (assembly.name != assemblyData.name) { continue; }

                Debug.Assert(assembly.sourceFiles != null);
                Debug.Assert(assembly.sourceFiles.Length > 0);
                Debug.Assert(File.Exists(assembly.outputPath));

                assemblyData.Source.Files = new SourceFile[assembly.sourceFiles.Length];

                for (int j = 0; j < assembly.sourceFiles.Length; j++)
                {
                    assemblyData.Source.Files[j].Path = assembly.sourceFiles[j];
                }

                AssetDatabase.ReleaseCachedFileHandles();

                // Swap out source code
                for (var j = 0; j < assembly.sourceFiles.Length; j++)
                {
                    long fileId;
                    string fileGuid;
                    var sourceFile = assembly.sourceFiles[j];

                    var sourceObject = AssetDatabase.LoadAssetAtPath<UnityObject>(sourceFile);

                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sourceObject, out fileGuid, out fileId))
                    {
                        assemblyData.Source.Files[j].Guid = fileGuid;
                        assemblyData.Source.Files[j].FileId = fileId;
                        ScriptGuidTable[fileGuid] = sourceObject.name;
                    }

                    var fullPath = Path.GetFullPath(sourceFile);
                    var newPath = fullPath.Hide();

                    EditorUtility.DisplayProgressBar("Replacing source with assembly", $"Processing file {Path.GetFileName(fullPath)}", j / (float)assembly.sourceFiles.Length);

                    File.Move(fullPath, newPath);
                    File.Move($"{fullPath}{META}", $"{newPath}{META}");
                }

                #region Import Dll

                var assemblyPath = $"{directoryPath}\\{assembly.name}{DLL}";

                EditorUtility.DisplayProgressBar("Replacing source with assembly", "Copying assembly into project...", 0.5f);

                File.Copy(assembly.outputPath, assemblyPath);

                EditorUtility.DisplayProgressBar("Replacing source with assembly", "Importing plugin...", 0.625f);

                AssetDatabase.ImportAsset(assemblyPath.GetUnityProjectRelativePath());

                EditorUtility.DisplayProgressBar("Replacing source with assembly", "Updating plugin settings...", 0.75f);

                var importedAssembly = (PluginImporter)AssetImporter.GetAtPath(assemblyPath.GetUnityProjectRelativePath());

                if (importedAssembly == null)
                {
                    Debug.LogError("Failed to get plugin importer!");
                    return true;
                }

                if (assemblyData.excludePlatforms != null && assemblyData.excludePlatforms.Length > 0 &&
                    assemblyData.includePlatforms != null && assemblyData.includePlatforms.Length > 0)
                {
                    Selection.activeObject = importedAssembly;
                    Debug.LogError("Unable to update plugin import settings, as both exclude and include platforms have been enabled.");
                    return true;
                }

                BuildTarget buildTarget;
                importedAssembly.SetCompatibleWithAnyPlatform(assemblyData.includePlatforms == null || assemblyData.includePlatforms.Length == 0);

                if (assemblyData.includePlatforms != null && assemblyData.includePlatforms.Length > 0)
                {
                    importedAssembly.SetCompatibleWithEditor(assemblyData.includePlatforms.Contains("Editor"));

                    for (int j = 0; j < assemblyData.includePlatforms?.Length; j++)
                    {
                        if (assemblyData.includePlatforms[j].TryGetBuildTarget(out buildTarget))
                        {
                            importedAssembly.SetCompatibleWithPlatform(buildTarget, true);
                        }
                    }
                }

                if (assemblyData.excludePlatforms != null && assemblyData.excludePlatforms.Length > 0)
                {
                    importedAssembly.SetCompatibleWithEditor(!assemblyData.excludePlatforms.Contains("Editor"));

                    for (int j = 0; j < assemblyData.excludePlatforms?.Length; j++)
                    {
                        if (assemblyData.excludePlatforms[j].TryGetBuildTarget(out buildTarget))
                        {
                            importedAssembly.SetExcludeFromAnyPlatform(buildTarget, true);
                        }
                    }
                }

                EditorUtility.DisplayProgressBar("Replacing source with assembly", "Saving and re-importing with updated settings...", 0.8f);

                importedAssembly.SaveAndReimport();

                #endregion Import Dll

                EditorUtility.DisplayProgressBar("Updating script references", "Updating gathering reference data...", 0.1f);

                string dllGuid;
                InitializeFileIdTable(assemblyPath, out dllGuid);
                UpdateAllAssetReferences(dllGuid, false);

                Selection.activeObject = importedAssembly;

                return true;
            }

            return false;
        }

        [MenuItem("CONTEXT/PluginImporter/Replace Assembly with Source", true, 99)]
        public static bool ReplaceWithSourceValidation()
        {
            if (Selection.activeObject == null) { return false; }

            Debug.Assert(EditorSettings.serializationMode == SerializationMode.ForceText, "Editor settings must use force text serialization for this process to work.");
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            return assetPath.GetAssetPathSiblings().Any(path => assetPath.Contains(DLL) || path.Contains(DLL));
        }

        [MenuItem("CONTEXT/PluginImporter/Replace Assembly with Source", false, 99)]
        public static void ReplaceWithSource()
        {
            Debug.Assert(Selection.activeObject != null);

            EditorUtility.DisplayProgressBar("Replacing assembly with source", "Getting things ready...", 0);

            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            string builtAssemblyPath = assetPath;
            string assemblyPath = assetPath.FindSiblingFileByExtension(ASMDEF);

            var asmdefFullPath = Path.GetFullPath(assemblyPath).UnHide();
            var asmdefHiddenPath = asmdefFullPath.Hide();

            File.Move(asmdefHiddenPath, asmdefFullPath);
            File.Move($"{asmdefHiddenPath}{META}", $"{asmdefFullPath}{META}");

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            EditorUtility.DisplayProgressBar("Replacing assembly with source", "Getting source file data...", .25f);

            Debug.Assert(!string.IsNullOrEmpty(builtAssemblyPath), "No Assembly found for this Assembly Definition!");
            Debug.Assert(!string.IsNullOrEmpty(asmdefFullPath), "No Assembly Definition found for this Assembly!");
            var assemblyDefinitionAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(asmdefFullPath.GetUnityProjectRelativePath());
            Debug.Assert(assemblyDefinitionAsset != null, $"Failed to load assembly def asset at {asmdefFullPath}");
            var assemblyDefinitionText = assemblyDefinitionAsset.text;
            var scriptAssemblyData = CustomScriptAssemblyData.FromJson(assemblyDefinitionText);
            var assemblySourcePath = $"{asmdefHiddenPath}{JSON}";
            Debug.Assert(File.Exists(assemblySourcePath), "Fatal Error: Missing meta data to re-import source files. You'll need to manually do it by removing the '.' in front of each file.");
            string sourceFilesText = File.ReadAllText(assemblySourcePath);
            File.Delete(assemblySourcePath);
            scriptAssemblyData.Source = JsonUtility.FromJson<AsmDefSourceFiles>(sourceFilesText);

            Debug.Assert(scriptAssemblyData != null);
            Debug.Assert(scriptAssemblyData.Source?.Files != null, "Fatal Error: Missing meta data to re-import source files. You'll need to manually do it by removing the '.' in front of each file.");

            AssetDatabase.ReleaseCachedFileHandles();

            for (var i = 0; i < scriptAssemblyData.Source.Files.Length; i++)
            {
                var sourceFile = scriptAssemblyData.Source.Files[i];
                var fullHiddenPath = Path.GetFullPath(sourceFile.Path).Hide();
                var sourceFileName = Path.GetFileNameWithoutExtension(sourceFile.Path);
                Debug.Assert(!string.IsNullOrEmpty(sourceFileName));

                if (!File.Exists(fullHiddenPath))
                {
                    Debug.LogError($"Failed to find source file at {sourceFile.Path}");
                    continue;
                }

                ScriptGuidTable[sourceFileName] = sourceFile.Guid;
                var sourcePath = fullHiddenPath.UnHide();

                EditorUtility.DisplayProgressBar("Replacing assembly with source", $"Processing file {Path.GetFileName(fullHiddenPath).UnHide()}", i / (float)scriptAssemblyData.Source.Files.Length);

                File.Move(fullHiddenPath, sourcePath);
                File.Move($"{fullHiddenPath}{META}", $"{sourcePath}{META}");
            }

            EditorUtility.DisplayProgressBar("Replacing assembly with source", "Deleting assembly...", .75f);

            string dllGuid;
            InitializeFileIdTable(builtAssemblyPath, out dllGuid);
            File.Delete(builtAssemblyPath);
            File.Delete($"{builtAssemblyPath}{META}");

            UpdateAllAssetReferences(dllGuid);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            EditorUtility.ClearProgressBar();
        }

        private static string Hide(this string path)
        {
            var index = path.LastIndexOf("\\", StringComparison.Ordinal);

            if (index == 0)
            {
                index = path.LastIndexOf("/", StringComparison.Ordinal);
            }

            if (index == 0)
            {
                return $".{path}";
            }

            return path.Insert(index + 1, ".");
        }

        private static string UnHide(this string path)
        {
            string fileName = Path.GetFileName(path);

            if (string.IsNullOrEmpty(fileName)) { return path; }
            if (!fileName.Contains(".")) { return path; }

            if (fileName.IndexOf(".", StringComparison.Ordinal) == 0)
            {
                fileName = fileName.TrimStart('.');
            }

            return path.Replace($"\\.{fileName}", $"\\{fileName}");
        }

        private static string GetUnityProjectRelativePath(this string fullPath)
        {
            return fullPath.Replace(Path.GetFullPath(Application.dataPath), "Assets").Replace("\\", "/");
        }

        private static string[] GetAssetPathSiblings(this string assetPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(assetPath), $"Invalid asset path {assetPath}");
            string directoryPath = new FileInfo(assetPath).Directory?.FullName;
            Debug.Assert(!string.IsNullOrEmpty(directoryPath), $"Invalid root path {directoryPath}");
            return Directory.GetFiles(directoryPath);
        }

        private static string FindSiblingFileByExtension(this string path, string extension)
        {
            var directoryPath = new FileInfo(path).Directory?.FullName;
            Debug.Assert(!string.IsNullOrEmpty(directoryPath));
            var files = Directory.GetFiles(directoryPath);

            for (var i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]) == extension)
                {
                    return files[i].GetUnityProjectRelativePath();
                }
            }

            return string.Empty;
        }

        private static bool TryGetBuildTarget(this string platform, out BuildTarget buildTarget)
        {
            switch (platform)
            {
                case "Editor":
                    buildTarget = BuildTarget.NoTarget;
                    return false;
                case "Android":
                    buildTarget = BuildTarget.Android;
                    return true;
                case "iOS":
                    buildTarget = BuildTarget.iOS;
                    return true;
                case "LinuxStandalone32":
                    buildTarget = BuildTarget.StandaloneLinux;
                    return true;
                case "LinuxStandalone64":
                    buildTarget = BuildTarget.StandaloneLinux64;
                    return true;
                case "LinuxStandaloneUniversal":
                    buildTarget = BuildTarget.StandaloneLinuxUniversal;
                    return true;
                case "macOSStandalone":
                    buildTarget = BuildTarget.StandaloneOSX;
                    return true;
                case "Nintendo3DS":
                    buildTarget = BuildTarget.N3DS;
                    return true;
                case "PS4":
                    buildTarget = BuildTarget.PS4;
                    return true;
                case "Switch":
                    buildTarget = BuildTarget.Switch;
                    return true;
                case "tvOS":
                    buildTarget = BuildTarget.tvOS;
                    return true;
                case "WSA":
                    buildTarget = BuildTarget.WSAPlayer;
                    return true;
                case "WebGL":
                    buildTarget = BuildTarget.WebGL;
                    return true;
                case "WindowsStandalone32":
                    buildTarget = BuildTarget.StandaloneWindows;
                    return true;
                case "WindowsStandalone64":
                    buildTarget = BuildTarget.StandaloneWindows64;
                    return true;
                case "XboxOne":
                    buildTarget = BuildTarget.XboxOne;
                    return true;
                default:
                    // If unsupported then it needs to be added to the switch statement above.
                    Debug.LogError($"{platform} unsupported!");
                    buildTarget = BuildTarget.NoTarget;
                    return false;
            }
        }

        private static void UpdateAllAssetReferences(string dllGuid, bool assemblyToSource = true)
        {
            Debug.Log("Attempting to update all references...");

            if (string.IsNullOrEmpty(dllGuid))
            {
                Debug.LogError("Cannot update references without valid information about the dll");
                return;
            }

            var assetFiles = FindAllAssets();

            Debug.Log($"Updating {assetFiles.Count} files...");

            for (int i = 0; i < assetFiles.Count; i++)
            {
                var filePath = assetFiles[i];
                Debug.Log($"Attempting to update {filePath} ...");

                EditorUtility.DisplayProgressBar("Updating References...", filePath, i * 1f / assetFiles.Count);

                int index = 0;
                bool replace = false;

                try
                {
                    string[] contents = File.ReadAllLines(filePath);

                    for (; index < contents.Length; ++index)
                    {
                        if (contents[index].StartsWith("MonoBehaviour:"))
                        {
                            do
                            {
                                ++index;
                            }
                            while (!contents[index].TrimStart().StartsWith("m_Script:"));

                            if (assemblyToSource)
                            {
                                replace |= ReplaceAssemblyReferencesWithSource(filePath, dllGuid, ref contents[index]);
                            }
                            else
                            {
                                replace |= ReplaceSourceReferencesWithAssembly(filePath, dllGuid, ref contents[index]);
                            }
                        }
                    }

                    if (!replace) { continue; }

                    File.WriteAllLines(filePath, contents);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    continue;
                }

                Debug.Log($"Updated {Path.GetFileNameWithoutExtension(filePath)}");
            }

            Debug.Log("Done updating references");
        }

        private static bool ReplaceSourceReferencesWithAssembly(string filePath, string dllGuid, ref string line)
        {
            bool replace = false;

            if (string.IsNullOrEmpty(dllGuid))
            {
                Debug.LogError("Cannot update references without valid information about the dll");
                return false;
            }

            string guid = line.GetGuidFromLine();

            if (string.IsNullOrEmpty(guid)) { return false; }

            string fileName;

            if (ScriptGuidTable.TryGetValue(guid, out fileName))
            {
                string dllFileId;

                if (FileIdTable.TryGetValue(fileName, out dllFileId))
                {
                    line = line.Replace(DEFAULT_SCRIPT_FILE_ID, dllFileId);
                    line = line.Replace(guid, dllGuid);
                    replace = true;
                }
                else
                {
                    Debug.LogWarning($"{filePath} | Can't find the GUID of file: {fileName}");
                }
            }
            else
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Debug.LogWarning($"{filePath} | Can't find the file of GUID: {guid} at {assetPath}");
            }

            return replace;
        }

        private static bool ReplaceAssemblyReferencesWithSource(string filePath, string dllGuid, ref string line)
        {
            bool replaced = false;

            string lineGuid = line.GetGuidFromLine();

            if (lineGuid != dllGuid) { return false; }

            string dllFileId = line.GetFileIdFromLine();

            if (string.IsNullOrEmpty(dllFileId) ||
                dllFileId.Equals(DEFAULT_SCRIPT_FILE_ID))
            {
                return false;
            }

            string scriptName;

            if (FileIdTable.TryGetValue(dllFileId, out scriptName))
            {
                string scriptGuid;
                if (ScriptGuidTable.TryGetValue(scriptName, out scriptGuid))
                {
                    line = line.Replace(dllFileId, DEFAULT_SCRIPT_FILE_ID);
                    line = line.Replace(dllGuid, scriptGuid);
                    replaced = true;
                }
                else
                {
                    Debug.LogWarning($"{filePath} | Can't find the GUID of {scriptName}");
                }
            }
            else
            {
                Debug.LogWarning($"{filePath} | Can't find the script name of file id: {dllFileId}");
            }

            return replaced;
        }

        private static string GetGuidFromLine(this string lineStr)
        {
            int startIndex = lineStr.IndexOf("guid:", StringComparison.Ordinal) + "guid: ".Length;
            int length = lineStr.LastIndexOf(",", StringComparison.Ordinal) - startIndex;
            return length <= 0 ? null : lineStr.Substring(startIndex, length);
        }

        private static string GetFileIdFromLine(this string lineStr)
        {
            int startIndex = lineStr.IndexOf("fileID:", StringComparison.Ordinal) + "fileID: ".Length;
            int length = lineStr.IndexOf(",", StringComparison.Ordinal) - startIndex;
            return length <= 0 ? null : lineStr.Substring(startIndex, length);
        }

        private static List<string> FindAllAssets()
        {
            var resultList = new List<string>();
            foreach (string extension in DefaultFileExtensions)
            {
                resultList.AddRange(Directory.GetFiles(Path.GetFullPath(Application.dataPath), extension, SearchOption.AllDirectories));
            }
            return resultList;
        }

        private static void InitializeFileIdTable(string assemblyPath, out string dllGuid)
        {
            dllGuid = null;
            var assemblyObjects = AssetDatabase.LoadAllAssetsAtPath(assemblyPath.GetUnityProjectRelativePath());

            for (var j = 0; j < assemblyObjects?.Length; j++)
            {
                long dllFileId;
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(assemblyObjects[j], out dllGuid, out dllFileId))
                {
                    FileIdTable[assemblyObjects[j].name] = dllFileId.ToString();
                }
            }
        }
    }
}
