using PantheonGames.TexturePacker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PG
{

    public static class UIModuleProcessor
    {
        public const string kUIModuleRootPath = "Assets/AssetBundles/UI/Modules";
        public const string kUIWidgetRootPath = "Assets/AssetBundles/UI/Widgets";
        public const string kUIGlobalRootPath = "Assets/AssetBundles/UI/Global";
        public const string kUIIconRootPath = "Assets/AssetBundles/UI/Icons";
        public const string kUIModelRootPath = "Assets/AssetBundles/Models";
        public const string kSoundsRootPath = "Assets/AssetBundles/Sounds";
        public const string kUIRootPath = "Assets/AssetBundles/UI/UIRoot";

        public const string kAtlasName = "Atlas";
        public const string kTextureName = "Textures";
        public const string kPrefabsName = "Prefabs";

        public static readonly string[] kAssetBundleIncluded = new string[] { kTextureName, "Animations", "Materials", kPrefabsName };

        public const string kActAtlasRootPath = "Assets/AssetBundles/UI/Modules/Activity/AtlasOfSprite";
        public const string kActPrefabRootPath = "Assets/AssetBundles/UI/Modules/Activity/Prefabs5.0";
        //[MenuItem("Assets/UIModule/Redirect Activity Prefab Atlas Referrence", false, 30)]
        static void RedirectActPrefabRef()
        {
            TextEditor t = new TextEditor();
            //get all atlas
            string[] guids_atlas = AssetDatabase.FindAssets("SpriteAtlas", new[] { kActAtlasRootPath });
            System.Collections.Generic.List<SpriteAtlas> atlas = new System.Collections.Generic.List<SpriteAtlas>();
            foreach (var guid in guids_atlas)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                SpriteAtlas sprite = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);

                if (sprite)
                    atlas.Add(sprite);
            }

            string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { kActPrefabRootPath });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log(path);
                UIModuleSwitchPrefabSprite.ApplyTexturePackerAtlas(path, atlas.ToArray());
            }
        }

        private static void DisplayProgressBar(string info, float progress)
        {
            EditorUtility.DisplayProgressBar(nameof(UIModuleProcessor), info, progress);
        }

        public static void FormatSpecificUIModule(string module)
        {
            AssetDatabase.Refresh();
            var path = kUIModuleRootPath + "/" + module + "/Atlas";
            var guids = AssetDatabase.FindAssets("t:texture", new[] { path });
            foreach (var guid in guids)
            {
                var filePath = AssetDatabase.GUIDToAssetPath(guid);
                try
                {
                    TextureFormatUtility.SetPreferredSpriteSettings(filePath, (settings, spriteSettings) =>
                    {
                        settings.spriteBorder = spriteSettings.spriteBorder;
                    }, importer => false);
                    TextureFormatUtility.SetPreferredPlatformSettings(filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {

                }
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/UIModule/Format UI(所有)", false, 1)]
        static void FormatUIModule()
        {
            var paths = GetSelectValidModulePaths(true);
            if (paths.Length > 0)
            {
                foreach (var path in paths)
                {
                    try
                    {
                        UIModuleAssetFormater.Run(path, DisplayProgressBar);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }
            }
            else Debug.Log("No valid path selected.");
        }

        // [MenuItem("Assets/UIModule/Format UI(仅用于UI-Icons-Item目录)", false, 1)]

        [MenuItem("Assets/UIModule/Process UI", false, 20)]
        static void ProcessUIModuleDefault()
        {
            var paths = GetSelectValidModulePaths();
            if (paths.Length > 0)
            {
                foreach (var path in paths)
                    ProcessUIModule(path, false);
            }
            else Debug.Log("No valid module path selected.");
        }

        [MenuItem("Assets/Battle/资源处理/Process Sprite Pivot", false, 10)]
        static void ProcessBattleSpritePivot()
        {
            HashSet<string> validFolders = new HashSet<string>();
            var selections = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets);
            foreach (var selection in selections)
            {
                if (!ProjectWindowUtil.IsFolder(selection.GetInstanceID())) continue;
                var path = AssetDatabase.GetAssetPath(selection);
                UIModuleSpriteAtlas.ModifySpritePivot(path);
            }
        }

        [MenuItem("Assets/UIModule/Other/Process UI(考虑global atlas)", false, 20)]
        public static void ProcessUIModuleWithGlobalAtlas()
        {
            var paths = GetSelectValidModulePaths();
            if (paths.Length > 0)
            {
                foreach (var path in paths)
                    ProcessUIModule(path, true);
            }
            else Debug.Log("No valid module path selected.");
        }

        public static void ProcessUIModule(string path, bool isConsiderGlobal)
        {
            if (IsValidModulePath(path))
            {
                try
                {
                    UIModuleAssetFormater.Run(path, DisplayProgressBar);
                    UIModuleSpriteAtlas.Run(path, DisplayProgressBar);
                    UIModuleSwitchPrefabSprite.Run(path, DisplayProgressBar, isConsiderGlobal);
                    //UIModuleAssetBundle.Run(path, DisplayProgressBar);
                    UIModuleCanvas.Run(path, DisplayProgressBar);
                    CheckUIModule(path);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
            else Debug.LogErrorFormat("UIModule of path {0} is not valid module.", path);
        }

        [MenuItem("Assets/UIModule/Check UI", false, 30)]
        static void ProcessUIModuleCheck()
        {
            var paths = GetSelectValidModulePaths();
            if (paths.Length > 0)
            {
                foreach (var path in paths)
                    CheckUIModule(path);
            }
            else Debug.Log("No valid module path selected.");
        }


        [MenuItem("Assets/UIModule/检查该prefab的被引用情况", false, 42)]
        static void CheckPrefabReference()
        {
            PrefabProcessTools.CheckSelectedPrefabsReferrence();
        }

        [MenuItem("UI Tools/CheckAllPrefabReference", false, 91)]
        static void CheckAllPrefabReference()
        {
            PrefabProcessTools.CheckAllPrefabsReferrence();
        }

        private static string[] GetSelectModulePath(string rootPath, bool includeTopPath, List<string> skipPaths)
        {
            HashSet<string> folders = new HashSet<string>();
            UnityEngine.Object[] selectObjs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
            var startIndex = rootPath.Length + 1;
            var skipPathCount = skipPaths == null ? 0 : skipPaths.Count;
            foreach (var obj in selectObjs)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (skipPathCount > 0)
                {
                    bool isContinue = false;
                    for (int i = 0; i < skipPathCount; ++i)
                    {
                        if (path.Contains(skipPaths[i]))
                        {
                            isContinue = true;
                            break;
                        }
                    }
                    if (isContinue) continue;
                }

                if (includeTopPath && path.CompareTo(rootPath) == 0)
                {
                    folders.Add(path);
                }
                else if (startIndex < path.Length && path.Contains(rootPath))
                {
                    var fileName = Path.GetFileName(path);
                    var subFolder = path.Substring(startIndex);
                    if (!AssetDatabase.IsValidFolder(path) && string.CompareOrdinal(fileName, subFolder) == 0) continue;
                    var index = path.IndexOf('/', startIndex);
                    if (index != -1) path = path.Substring(0, index);
                    folders.Add(path);
                }
            }
            return folders.ToArray();
        }

        public static void CheckUIModule(string path)
        {
            if (IsValidModulePath(path))
            {
                try
                {
                    UIModulePrefabSpriteChecker.Run(path, DisplayProgressBar);
                    UIModuleAnimatorChecker.Run(path, DisplayProgressBar);
                    UIModuleScrollRectCanvasChecker.Run(path, DisplayProgressBar);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
            else Debug.LogErrorFormat("UIModule of path {0} is not valid module.", path);
        }

        static string[] GetSelectValidModulePaths(bool selectAll = false)
        {
            HashSet<string> validFolders = new HashSet<string>();
            var selections = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets);
            foreach (var selection in selections)
            {
                if (!ProjectWindowUtil.IsFolder(selection.GetInstanceID())) continue;
                var path = AssetDatabase.GetAssetPath(selection);
                if (path.StartsWith(kUIModuleRootPath))
                {
                    if (FindRootPath(kUIModuleRootPath, ref path))
                        validFolders.Add(path);
                }
                else if (selectAll)
                {
                    if (path.StartsWith(kUIGlobalRootPath))
                        validFolders.Add(kUIGlobalRootPath);
                    else if (path.StartsWith(kUIWidgetRootPath))
                    {
                        if (FindRootPath(kUIWidgetRootPath, ref path))
                            validFolders.Add(path);
                    }
                    else if (path.StartsWith(kUIIconRootPath))
                        validFolders.Add(kUIIconRootPath);
                }
            }
            return validFolders.ToArray();
        }

        static string[] GetSelectValidModulePathsTemp()
        {
            HashSet<string> validFolders = new HashSet<string>();
            var selections = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets);
            foreach (var selection in selections)
            {
                if (!ProjectWindowUtil.IsFolder(selection.GetInstanceID())) continue;
                var path = AssetDatabase.GetAssetPath(selection);
                validFolders.Add(path);
            }
            return validFolders.ToArray();
        }

        static bool FindRootPath(string rootPath, ref string path)
        {
            if (rootPath.Length + 1 >= path.Length) return false;
            var index = path.IndexOf('/', rootPath.Length + 1);
            if (index != -1)
                path = path.Substring(0, index);
            return true;
        }

        static bool IsValidModulePath(string path)
        {
            return path.StartsWith(kUIModuleRootPath);
        }

        static List<string> s_SplitPaths = new List<string>();
        public static string GetComponentPath(Component component)
        {
            s_SplitPaths.Clear();
            s_SplitPaths.Add(component.GetType().Name);
            s_SplitPaths.Add(component.name);
            Transform parent = component.transform.parent;
            while (parent)
            {
                s_SplitPaths.Add(parent.name);
                parent = parent.parent;
            }
            s_SplitPaths.Reverse();
            var result = string.Join("/", s_SplitPaths);
            s_SplitPaths.Clear();
            return result;
        }

        public static bool IsPropertyTypeMatched(SerializedProperty property, Type type)
        {
            var propertyInfo = typeof(SerializedProperty).GetProperty("objectReferenceTypeString", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var propertyTypeStr = propertyInfo.GetValue(property) as string;
            return propertyTypeStr == type.Name || propertyTypeStr == type.BaseType.Name;
        }
    }
}
