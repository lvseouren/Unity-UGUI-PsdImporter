using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PG
{
    public static class UIModuleScrollRectCanvasChecker
    {
        public static void Run(string path, Action<string, float> callback)
        {
            callback?.Invoke("Prepare ScrollRect Canvas Check...", 0);
            var guids = AssetDatabase.FindAssets("t:prefab", new[] { path });
            for (int i = 0; i < guids.Length; i++)
            {
                callback?.Invoke($"Check ScrollRect Canvas...{i + 1}/{guids.Length}", (i + 1) / (float)guids.Length);
                var prefabPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                CheckScrollRectCanvas(prefabPath);
            }
        }

        static void CheckScrollRectCanvas(string path)
        {
            GameObject go = PrefabUtility.LoadPrefabContents(path);
            ScrollRect[] scrolls = go.GetComponentsInChildren<ScrollRect>(true);
            foreach (var scroll in scrolls)
            {
                var canvas = scroll.GetComponent<Canvas>();
                if (canvas)
                {
                    Debug.LogErrorFormat("It is not pretty good to put Canvas under ScrollRect, please move it to ScrollRect.content instead.\nPrefab Path: {0}\nComponent Path: {1}", path, UIModuleProcessor.GetComponentPath(scroll));
                    continue;
                }
                if (!scroll.content)
                {
                    Debug.LogErrorFormat("ScrollRect missing content...\nPrefab Path: {0}\nComponent Path: {1}", path, UIModuleProcessor.GetComponentPath(scroll));
                    continue;
                }
                canvas = scroll.content.GetComponent<Canvas>();
                if (!canvas)
                    Debug.LogErrorFormat("ScrollRect'content need Canvas.\nPrefab Path: {0}\nComponent Path: {1}", path, UIModuleProcessor.GetComponentPath(scroll.content));
                else if (!scroll.content.GetComponent<GraphicRaycaster>())
                    Debug.LogErrorFormat("ScollRect'content missing GraphicRaycaster.\nPrefab Path: {0}\nComponent Path: {1}", path, UIModuleProcessor.GetComponentPath(scroll.content));
            }
            PrefabUtility.UnloadPrefabContents(go);
        }
    }
}
