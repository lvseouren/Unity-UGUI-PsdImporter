using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PG
{
    public static class UIModuleAssetFormater
    {
        enum UIFormatType
        {
            Texture,
            Sprite
        }

        struct UIFormatInfo
        {
            public string filePath;
            public UIFormatType formatType;
        }

        public static void Run(string path, Action<string, float> callback)
        {
            callback?.Invoke("Calculate UI Textures...", 0);
            List<UIFormatInfo> formatInfo = new List<UIFormatInfo>();
            CalculateFormatInfo(path, formatInfo);
            for (int i = 0; i < formatInfo.Count; i++)
            {
                callback?.Invoke($"Formatting UI Textures...{i + 1}/{formatInfo.Count}", (i + 1) / (float)formatInfo.Count);
                var fInfo = formatInfo[i];
                if (fInfo.formatType == UIFormatType.Sprite)
                {
                    TextureFormatUtility.SetPreferredSpriteSettings(fInfo.filePath, (settings, spriteSettings) =>
                    {
                        settings.spriteBorder = spriteSettings.spriteBorder;
                    }, importer => false);
                    TextureFormatUtility.SetPreferredPlatformSettings(fInfo.filePath);
                }
                else if (fInfo.formatType == UIFormatType.Texture)
                {
                    TextureFormatUtility.SetPreferredTextureSettings(fInfo.filePath);
                    TextureFormatUtility.SetPreferredPlatformSettings(fInfo.filePath, (apply, revert) =>
                    {
                        TextureImporter importer = AssetImporter.GetAtPath(fInfo.filePath) as TextureImporter;
                        if (!importer.DoesSourceTextureHaveAlpha())
                        {
                            if (apply.name == "Android") apply.format = TextureImporterFormat.ETC2_RGB4;
                            else if (apply.name == "iPhone") apply.format = TextureImporterFormat.ASTC_6x6;
                        }
                    });
                    CheckTextureValidate(fInfo.filePath);
                }
            }
        }

        public static void CheckTextureValidate(string path)
        {
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (texture.width % 4 != 0 || texture.height % 4 != 0)
                Debug.LogWarningFormat("The texture [{0}]'s width/height must be multiple of 4", path);
        }

        static void CalculateFormatInfo(string path, List<UIFormatInfo> formatInfo)
        {
            string[] folders = AssetDatabase.GetSubFolders(path);
            foreach (var folder in folders)
            {
                var dirName = Path.GetFileNameWithoutExtension(folder);
                if (dirName.StartsWith(UIModuleProcessor.kAtlasName))
                    CalculateFormatInfo(UIFormatType.Sprite, folder, formatInfo);
                else if (dirName.StartsWith(UIModuleProcessor.kTextureName) || path.Equals(UIModuleProcessor.kUIIconRootPath))
                    CalculateFormatInfo(UIFormatType.Texture, folder, formatInfo);
            }
        }

        static void CalculateFormatInfo(UIFormatType formatType, string path, List<UIFormatInfo> formatInfo)
        {
            var guids = AssetDatabase.FindAssets("t:texture", new[] { path });
            foreach (var guid in guids)
            {
                var filePath = AssetDatabase.GUIDToAssetPath(guid);
                formatInfo.Add(new UIFormatInfo { filePath = filePath, formatType = formatType });
            }
        }
    }
}
