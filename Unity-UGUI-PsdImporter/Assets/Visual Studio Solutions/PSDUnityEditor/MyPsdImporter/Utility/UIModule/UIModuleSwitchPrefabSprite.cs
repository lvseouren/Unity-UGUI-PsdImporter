using PantheonGames.TexturePacker;
using System;
using System.IO;
using PantheonGamesEditor.TexturePacker;
using UnityEditor;
using UnityEngine;

namespace PG
{
    public static class UIModuleSwitchPrefabSprite
    {
        public static void Run(string path, Action<string, float> callback, bool isConsiderGlobalAtlas)
        {
            callback?.Invoke("Find UI Prefabs...", 0);
            var guids = AssetDatabase.FindAssets("t:PantheonGames.TexturePacker.SpriteAtlas", new[] { path });
            if (guids.Length > 1)
                throw new InvalidOperationException($"{path} contains {guids.Length} SpriteAtlas, this is not allowed.");
            else if (guids.Length == 0)
                throw new InvalidOperationException($"{path} have no SpriteAtlas.");
            SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guids[0]));
            SpriteAtlas globalAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(UIModuleProcessor.kUIGlobalRootPath + "/SpriteAtlas.asset"));
            guids = AssetDatabase.FindAssets("t:prefab", new[] { path });
            for (int i = 0; i < guids.Length; i++)
            {
                callback?.Invoke($"Apply UI Prefabs...{i + 1}/{guids.Length}", (i + 1) / (float)guids.Length);
                var prefabPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                ApplyTexturePackerAtlas(prefabPath, spriteAtlas, isConsiderGlobalAtlas ? globalAtlas : null);
            }
        }

        static void ApplyTexturePackerAtlas(string path, SpriteAtlas spriteAtlas, SpriteAtlas globalAtlas)
        {
            GameObject go = PrefabUtility.LoadPrefabContents(path);
            bool isDirty = false;
            Component[] components = go.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (!component) continue;
                using (SerializedObject so = new SerializedObject(component))
                {
                    so.Update();
                    SerializedProperty property = so.GetIterator();
                    while (property.NextVisible(true))
                    {
                        if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceInstanceIDValue != 0 && UIModuleProcessor.IsPropertyTypeMatched(property, typeof(Sprite)))
                        { 
                            bool isReplaced = TryReplaceSpriteRefToAtlas(property, spriteAtlas);
                            if (!isReplaced && globalAtlas)
                                isReplaced = TryReplaceSpriteRefToAtlas(property, globalAtlas);
                            isDirty |= isReplaced;
                        }
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            if (isDirty) PrefabUtility.SaveAsPrefabAsset(go, path);
            PrefabUtility.UnloadPrefabContents(go);
        }

        public static void ApplyTexturePackerAtlas(string path, SpriteAtlas[] spriteAtlas)
        {
            GameObject go = PrefabUtility.LoadPrefabContents(path);
            bool isDirty = false;
            Component[] components = go.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (!component) continue;
                using (SerializedObject so = new SerializedObject(component))
                {
                    so.Update();
                    SerializedProperty property = so.GetIterator();
                    while (property.NextVisible(true))
                    {
                        if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceInstanceIDValue != 0 && UIModuleProcessor.IsPropertyTypeMatched(property, typeof(Sprite)))
                        {
                            foreach (var atlas in spriteAtlas)
                                isDirty |= TryReplaceSpriteRefToAtlas(property, atlas);
                        }
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            if (isDirty) PrefabUtility.SaveAsPrefabAsset(go, path);
            PrefabUtility.UnloadPrefabContents(go);
        }

        static bool TryReplaceSpriteRefToAtlas(SerializedProperty property, SpriteAtlas spriteAtlas)
        {
            if (property == null || property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue == null || property.objectReferenceValue.GetType() != typeof(Sprite))
                return false;
            var sprite = spriteAtlas.GetSprite(property.objectReferenceValue.name);
            if (sprite)
            {
                property.objectReferenceValue = sprite;
                return true;
            }

            var propertyPath = AssetDatabase.GetAssetPath(property.objectReferenceInstanceIDValue);
            if (File.Exists(Path.ChangeExtension(propertyPath, ".tpsheet"))) return false;
            var packedInstanceId = GetPackedSpriteInstanceId(propertyPath);
            if (packedInstanceId != 0)
            {
                property.objectReferenceInstanceIDValue = packedInstanceId;
                return true;
            }
            return false;
        }

        static int GetPackedSpriteInstanceId(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            return GetPackedSpriteInstanceId(Path.GetDirectoryName(path), fileName);
        }

        static int GetPackedSpriteInstanceId(string path, string spriteName)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            var guids = AssetDatabase.FindAssets("t:PantheonGames.TexturePacker.SpriteAtlas", new[] {path});
            foreach (var guid in guids)
            {
                var spriteAtlasPath = AssetDatabase.GUIDToAssetPath(guid);
                var dirPath = Path.GetDirectoryName(spriteAtlasPath);
                if (dirPath.Replace('\\', '/') == path.Replace('\\', '/'))
                {
                    var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(spriteAtlasPath);
                    var sheetPath = Path.Combine(path, spriteAtlas.name + TPGenerator.kTexturePackerSheetExtension);
                    if (File.Exists(sheetPath))
                    {
                        var allAssets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
                        foreach (var asset in allAssets)
                        {
                            if (asset is Sprite && asset.name == spriteName)
                                return asset.GetInstanceID();
                        }
                        return 0;
                    }
                    break;
                }
            }
            return GetPackedSpriteInstanceId(Path.GetDirectoryName(path), spriteName);
        }
    }
}
