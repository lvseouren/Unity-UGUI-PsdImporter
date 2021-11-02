using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PG
{
    public static class UIModulePrefabSpriteChecker
    {
        public static void Run(string path, Action<string, float> callback)
        {
            callback?.Invoke("Prepare Prefab Sprite Check...", 0);
            var guids = AssetDatabase.FindAssets("t:prefab", new[] { path });
            for (int i = 0; i < guids.Length; i++)
            {
                callback?.Invoke($"Check Prefab Sprite...{i + 1}/{guids.Length}", (i + 1) / (float)guids.Length);
                var prefabPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                CheckPrefabSprite(prefabPath);
            }
        }

        static void CheckPrefabSprite(string path)
        {
            GameObject go = PrefabUtility.LoadPrefabContents(path);
            Component[] components = go.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (!component) continue;
                using (SerializedObject so = new SerializedObject(component))
                {
                    SerializedProperty property = so.GetIterator();
                    while (property.NextVisible(true))
                    {
                        if (property.propertyType == SerializedPropertyType.ObjectReference
                            && property.objectReferenceInstanceIDValue != 0
                            && UIModuleProcessor.IsPropertyTypeMatched(property, typeof(Sprite))
                            && property.objectReferenceValue.GetType() == typeof(Sprite))
                        {
                            // exclude mask
                            if (component.GetComponent<Mask>()) continue;
                            var spritePath = AssetDatabase.GetAssetPath(property.objectReferenceInstanceIDValue);
                            var tpPath = Path.ChangeExtension(spritePath,  ".tpsheet");
                            if (!File.Exists(tpPath))
                                Debug.LogErrorFormat("The Sprite[{0}] must be packed into tpsheet.\nPrefab Path: {1}\nComponent Path: {2}\nProperty Path: {3}", spritePath, path, UIModuleProcessor.GetComponentPath(component), property.propertyPath);
                        }
                    }
                }
            }
            PrefabUtility.UnloadPrefabContents(go);
        }
    }
}
