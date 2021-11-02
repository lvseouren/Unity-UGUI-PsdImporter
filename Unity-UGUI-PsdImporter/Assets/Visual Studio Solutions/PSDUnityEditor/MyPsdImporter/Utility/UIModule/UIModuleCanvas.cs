using System;
using UnityEditor;
using UnityEngine;

namespace PG
{
    public static class UIModuleCanvas
    {
        public static void Run(string path, Action<string, float> callback)
        {
            string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { path });
            for (int i = 0; i < guids.Length; i++)
            {
                callback?.Invoke($"Updating Canvases...{i + 1}/{guids.Length}", (i + 1) / (float)guids.Length);
                var prefabPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var prefab = PrefabUtility.LoadPrefabContents(prefabPath);

                bool isDirty = false;
                var canvases = prefab.GetComponentsInChildren<Canvas>();
                foreach (var canvas in canvases)
                {
                    if (!canvas.additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord1))
                    {
                        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
                        isDirty = true;
                    }
                }
                if (isDirty)
                    PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }
    }
}
