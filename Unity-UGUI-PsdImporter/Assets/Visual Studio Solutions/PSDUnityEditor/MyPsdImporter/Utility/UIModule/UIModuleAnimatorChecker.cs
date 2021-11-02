using System;
using UnityEditor;
using UnityEngine;

namespace PG
{
    public static class UIModuleAnimatorChecker
    {
        public static void Run(string path, Action<string, float> callback)
        {
            callback?.Invoke("Prepare Animator Check...", 0);
            var guids = AssetDatabase.FindAssets("t:prefab", new[] { path });
            for (int i = 0; i < guids.Length; i++)
            {
                callback?.Invoke($"Check Prefab Animator...{i + 1}/{guids.Length}", (i + 1)/(float)guids.Length);
                var prefabPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject go = PrefabUtility.LoadPrefabContents(prefabPath);
                var animators = go.GetComponentsInChildren<Animator>(true);
                if (animators.Length > 0)
                {
                    foreach (var animator in animators)
                        Debug.LogErrorFormat("Do not use animator in UI prefab, please use animation instead.\nPrefab Path: {0}\nComponent Path: {1}", prefabPath, UIModuleProcessor.GetComponentPath(animator));
                }
                PrefabUtility.UnloadPrefabContents(go);
            }
        }
    }
}
