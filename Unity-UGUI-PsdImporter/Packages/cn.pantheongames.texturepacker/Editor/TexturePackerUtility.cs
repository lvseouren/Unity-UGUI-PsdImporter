using PantheonGames.TexturePacker;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PantheonGamesEditor.TexturePacker
{
    public static class TexturePackerUtility
    {
        public static bool IsPackable(SpriteAtlas spriteAtlas)
        {
            var packable = true;
            for (var i = 0; i < spriteAtlas.Objects.Length; i++)
            {
                var asset = spriteAtlas.Objects[i];
                packable = IsPackable(asset) && !Contains(spriteAtlas.Objects, asset, i);
                if (!packable) break;
            }
            return packable;
        }

        public static bool Contains(Object[] assets, Object asset, int index = -1)
        {
            if (index < 0) index = assets.Length;
            for (var i = 0; i < index; i++)
            {
                if (assets[i] == asset)
                    return true;
            }
            return false;
        }

        public static bool IsPackable(Object asset)
        {
            if (!asset) return false;
            if (asset is DefaultAsset)
                return ProjectWindowUtil.IsFolder(asset.GetInstanceID());
            var path = AssetDatabase.GetAssetPath(asset);
            return IsPackable(path);
        }

        public static bool IsPackable(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return true;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            return importer;
        }

        public static void CalculateSpriteNames(Object asset, Dictionary<string, HashSet<string>> cachedPaths)
        {
            if (cachedPaths == null) return;
            var path = AssetDatabase.GetAssetPath(asset);
            if (asset is DefaultAsset)
            {
                var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });
                foreach (var guid in guids)
                {
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    CalculateSpriteNames(path, cachedPaths);
                }
            }
            else if (asset is Sprite)
                InsertFileName(cachedPaths, path, asset.name);
            else
                CalculateSpriteNames(path, cachedPaths);
        }

        static void CalculateSpriteNames(string path, Dictionary<string, HashSet<string>> cachedPaths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer)
            {
                if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Multiple)
                {
                    var sheets = importer.spritesheet;
                    foreach (var sheet in sheets)
                        InsertFileName(cachedPaths, path, sheet.name);
                }
                else
                    InsertFileName(cachedPaths, path);
            }
        }

        public static Dictionary<string, HashSet<string>> GetSpritePaths(SpriteAtlas spriteAtlas)
        {
            return GetSpritePaths(spriteAtlas.Objects);
        }

        public static Dictionary<string, HashSet<string>> GetSpritePaths(Object[] assets)
        {
            var paths = new Dictionary<string, HashSet<string>>();
            foreach (var asset in assets)
                GetSpritePaths(asset, paths);
            return paths;
        }

        static void GetSpritePaths(Object asset, Dictionary<string, HashSet<string>> paths)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            if (asset is DefaultAsset)
            {
                var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });
                foreach (var guid in guids)
                {
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    InsertFilePath(paths, path);
                }
            }
            else if (asset is Sprite)
                InsertFilePath(paths, path, asset.name);
            else
                InsertFilePath(paths, path);
        }

        static void InsertFileName(Dictionary<string, HashSet<string>> dic, string path, string newName = null)
        {
            var fileName = newName ?? Path.GetFileNameWithoutExtension(path);
            if (dic.TryGetValue(fileName, out HashSet<string> paths))
                paths.Add(path);
            else
                dic.Add(fileName, new HashSet<string> { path });
        }

        static void InsertFilePath(Dictionary<string, HashSet<string>> dic, string path, string name = null)
        {
            if (dic.TryGetValue(path, out HashSet<string> names))
            {
                if (names.Count == 0)
                    return;
                if (!string.IsNullOrEmpty(name))
                    names.Add(name);
                else
                    names.Clear();
            }
            else
            {
                names = new HashSet<string>();
                dic.Add(path, names);
                if (!string.IsNullOrEmpty(name))
                    names.Add(name);
            }
        }

        internal static bool HaveSameNames(Dictionary<string, HashSet<string>> paths, Dictionary<string, HashSet<string>> collections = null)
        {
            HashSet<string> fileNames = null;
            if (collections == null)
                fileNames = new HashSet<string>();
            foreach (var kv in paths)
            {
                var importer = AssetImporter.GetAtPath(kv.Key) as TextureImporter;
                if (!importer) continue;

                if (collections != null)
                {
                    if (kv.Value.Count > 0)
                    {
                        foreach (var name in kv.Value)
                            InsertFileName(collections, kv.Key, name);
                    }
                    else if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Multiple)
                    {
                        var sheets = importer.spritesheet;
                        foreach (var sheet in sheets)
                            InsertFileName(collections, kv.Key, sheet.name);
                    }
                    else InsertFileName(collections, kv.Key);
                }
                else if (fileNames != null)
                {
                    if (kv.Value.Count > 0)
                    {
                        foreach (var name in kv.Value)
                        {
                            if (fileNames.Contains(name))
                                return true;
                            else
                                fileNames.Add(name);
                        }
                    }
                    else if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Multiple)
                    {
                        var sheets = importer.spritesheet;
                        foreach (var sheet in sheets)
                        {
                            if (fileNames.Contains(sheet.name))
                                return true;
                            else
                                fileNames.Add(sheet.name);
                        }
                    }
                    else
                    {
                        var fileName = Path.GetFileNameWithoutExtension(kv.Key);
                        if (fileNames.Contains(fileName))
                            return true;
                        else
                            fileNames.Add(fileName);
                    }
                }
            }
            if (collections != null)
            {
                foreach (var v in collections.Values)
                {
                    if (v.Count > 1) return true;
                }
            }
            return false;
        }
    }
}
