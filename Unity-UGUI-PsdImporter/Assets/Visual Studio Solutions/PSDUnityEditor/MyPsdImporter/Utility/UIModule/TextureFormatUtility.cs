using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TextureCompressionQuality = UnityEditor.TextureCompressionQuality;

namespace PG
{
    public static class TextureFormatUtility
    {
        public const float kDefaultPixelsPerUnit = 100;
        public static readonly Vector2 kDefaultPivot = new Vector2(0.5f, 0.5f);

        private static readonly TextureImporterSettings s_DefaultSettings = new TextureImporterSettings
        {
            alphaIsTransparency = false,
            alphaSource = TextureImporterAlphaSource.FromInput,
            alphaTestReferenceValue = 0.5f,
            aniso = -1,
            borderMipmap = false,
            convertToNormalMap = false,
            cubemapConvolution = TextureImporterCubemapConvolution.None,
            fadeOut = false,
            filterMode = (FilterMode)(-1),
            generateCubemap = TextureImporterGenerateCubemap.AutoCubemap,
            heightmapScale = 0.25f,
            mipMapsPreserveCoverage = false,
            mipmapBias = -1,
            mipmapEnabled = true,
            mipmapFadeDistanceEnd = 3,
            mipmapFadeDistanceStart = 1,
            mipmapFilter = TextureImporterMipFilter.BoxFilter,
            normalMapFilter = TextureImporterNormalFilter.Standard,
            npotScale = TextureImporterNPOTScale.ToNearest,
            readable = false,
            sRGBTexture = true,
            seamlessCubemap = false,
            spriteAlignment = (int)SpriteAlignment.Center,
            spriteBorder = Vector4.zero,
            spriteExtrude = 1,
            spriteGenerateFallbackPhysicsShape = true,
            spriteMeshType = SpriteMeshType.Tight,
            spriteMode = (int)SpriteImportMode.None,
            spritePivot = kDefaultPivot,
            spritePixelsPerUnit = kDefaultPixelsPerUnit,
            spriteTessellationDetail = -1,
            textureShape = TextureImporterShape.Texture2D,
            textureType = TextureImporterType.Default,
            wrapMode = (TextureWrapMode)(-1),
            wrapModeU = (TextureWrapMode)(-1),
            wrapModeV = (TextureWrapMode)(-1),
            wrapModeW = (TextureWrapMode)(-1),
        };

        private static readonly TextureImporterSettings s_PreferredSpriteSettings = new TextureImporterSettings
        {
            alphaIsTransparency = true,
            alphaSource = TextureImporterAlphaSource.FromInput,
            alphaTestReferenceValue = 0.5f,
            aniso = -1,
            borderMipmap = false,
            convertToNormalMap = false,
            cubemapConvolution = TextureImporterCubemapConvolution.None,
            fadeOut = false,
            filterMode = FilterMode.Bilinear,
            generateCubemap = TextureImporterGenerateCubemap.AutoCubemap,
            heightmapScale = 0.25f,
            mipMapsPreserveCoverage = false,
            mipmapBias = -1,
            mipmapEnabled = false,
            mipmapFadeDistanceEnd = 3,
            mipmapFadeDistanceStart = 1,
            mipmapFilter = TextureImporterMipFilter.BoxFilter,
            normalMapFilter = TextureImporterNormalFilter.Standard,
            npotScale = TextureImporterNPOTScale.None,
            readable = false,
            sRGBTexture = true,
            seamlessCubemap = false,
            spriteAlignment = (int)SpriteAlignment.Center,
            spriteBorder = Vector4.zero,
            spriteExtrude = 1,
            spriteGenerateFallbackPhysicsShape = true,
            spriteMeshType = SpriteMeshType.FullRect,
            spriteMode = (int)SpriteImportMode.Single,
            spritePivot = kDefaultPivot,
            spritePixelsPerUnit = kDefaultPixelsPerUnit,
            spriteTessellationDetail = -1,
            textureShape = TextureImporterShape.Texture2D,
            textureType = TextureImporterType.Sprite,
            wrapMode = TextureWrapMode.Clamp,
            wrapModeU = TextureWrapMode.Clamp,
            wrapModeV = TextureWrapMode.Clamp,
            wrapModeW = TextureWrapMode.Clamp,
        };

        private static readonly TextureImporterSettings s_PreferredTextureSettings = new TextureImporterSettings
        {
            alphaIsTransparency = true,
            alphaSource = TextureImporterAlphaSource.FromInput,
            alphaTestReferenceValue = 0.5f,
            aniso = -1,
            borderMipmap = false,
            convertToNormalMap = false,
            cubemapConvolution = TextureImporterCubemapConvolution.None,
            fadeOut = false,
            filterMode = FilterMode.Bilinear,
            generateCubemap = TextureImporterGenerateCubemap.AutoCubemap,
            heightmapScale = 0.25f,
            mipMapsPreserveCoverage = false,
            mipmapBias = -1,
            mipmapEnabled = false,
            mipmapFadeDistanceEnd = 3,
            mipmapFadeDistanceStart = 1,
            mipmapFilter = TextureImporterMipFilter.BoxFilter,
            normalMapFilter = TextureImporterNormalFilter.Standard,
            npotScale = TextureImporterNPOTScale.None,
            readable = false,
            sRGBTexture = true,
            seamlessCubemap = false,
            spriteAlignment = (int)SpriteAlignment.Center,
            spriteBorder = Vector4.zero,
            spriteExtrude = 1,
            spriteGenerateFallbackPhysicsShape = true,
            spriteMeshType = SpriteMeshType.Tight,
            spriteMode = (int)SpriteImportMode.None,
            spritePivot = kDefaultPivot,
            spritePixelsPerUnit = kDefaultPixelsPerUnit,
            spriteTessellationDetail = -1,
            textureShape = TextureImporterShape.Texture2D,
            textureType = TextureImporterType.Default,
            wrapMode = TextureWrapMode.Clamp,
            wrapModeU = TextureWrapMode.Clamp,
            wrapModeV = TextureWrapMode.Clamp,
            wrapModeW = TextureWrapMode.Clamp,
        };

        private static readonly Dictionary<string, TextureImporterPlatformSettings> s_DefaultPlatformSettings = new Dictionary<string, TextureImporterPlatformSettings>
        {
            {
                "Android",
                new TextureImporterPlatformSettings
                {
                    name = "Android",
                    allowsAlphaSplitting = false,
                    compressionQuality = (int)TextureCompressionQuality.Normal,
                    crunchedCompression = false,
                    maxTextureSize = 2048,
                    overridden = false,
                    textureCompression = TextureImporterCompression.Compressed,
                    format = TextureImporterFormat.Automatic,
                    androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings,
                    resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                }
            },
            {
                "iPhone",
                new TextureImporterPlatformSettings
                {
                    name = "iPhone",
                    allowsAlphaSplitting = false,
                    compressionQuality = (int)TextureCompressionQuality.Normal,
                    crunchedCompression = false,
                    maxTextureSize = 2048,
                    overridden = false,
                    textureCompression = TextureImporterCompression.Compressed,
                    format = TextureImporterFormat.Automatic,
                    androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings,
                    resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                }
            }
        };

        private static readonly Dictionary<string, TextureImporterPlatformSettings> s_PreferredPlatformSettings = new Dictionary<string, TextureImporterPlatformSettings>()
        {
            {
                "Android",
                new TextureImporterPlatformSettings
                {
                    name = "Android",
                    allowsAlphaSplitting = true,
                    compressionQuality = (int)TextureCompressionQuality.Normal,
                    crunchedCompression = false,
                    maxTextureSize = 2048,
                    overridden = true,
                    textureCompression = TextureImporterCompression.Compressed,
                    format = TextureImporterFormat.ETC2_RGBA8,
                    androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings,
                    resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                }
            },
            {
                "iPhone",
                new TextureImporterPlatformSettings
                {
                    name = "iPhone",
                    allowsAlphaSplitting = true,
                    compressionQuality = (int)TextureCompressionQuality.Normal,
                    crunchedCompression = false,
                    maxTextureSize = 2048,
                    overridden = true,
                    textureCompression = TextureImporterCompression.Compressed,
                    format = TextureImporterFormat.ASTC_6x6,
                    androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings,
                    resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                }
            }
        };

        public delegate bool PostImportTextureSettings(TextureImporter importer);

        public static TextureImporterPlatformSettings GetDefaultPlatformSettings(string platform)
        {
            TextureImporterPlatformSettings defaultSettings;
            if (s_DefaultPlatformSettings.TryGetValue(platform, out defaultSettings))
            {
                TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
                defaultSettings.CopyTo(settings);
                return settings;
            }
            return null;
        }

        public static TextureImporterPlatformSettings GetPreferredPlatformSettings(string platform)
        {
            TextureImporterPlatformSettings preferredSettings;
            if (s_PreferredPlatformSettings.TryGetValue(platform, out preferredSettings))
            {
                TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
                preferredSettings.CopyTo(settings);
                return settings;
            }
            return null;
        }

        public static bool SetDefaultPlatformSettings(string path)
        {
            return SetDefaultPlatformSettings(path, s_DefaultPlatformSettings.Keys.ToArray());
        }

        public static bool SetPreferredPlatformSettings(string path, Action<TextureImporterPlatformSettings, TextureImporterPlatformSettings> onPreimport = null)
        {
            return SetPreferredPlatformSettings(path, onPreimport, s_PreferredPlatformSettings.Keys.ToArray());
        }

        public static bool SetDefaultPlatformSettings(string path, params string[] platforms)
        {
            List<TextureImporterPlatformSettings> platformSettings = new List<TextureImporterPlatformSettings>();
            foreach (string platform in platforms)
            {
                TextureImporterPlatformSettings settings = GetDefaultPlatformSettings(platform);
                if (settings != null) platformSettings.Add(settings);
            }
            if (platformSettings.Count == 0) return false;
            return SetPlatformSettings(path, null, platformSettings.ToArray());
        }

        public static bool SetPreferredPlatformSettings(string path, Action<TextureImporterPlatformSettings, TextureImporterPlatformSettings> onPreimport = null, params string[] platforms)
        {
            List<TextureImporterPlatformSettings> platformSettings = new List<TextureImporterPlatformSettings>();
            foreach (string platform in platforms)
            {
                TextureImporterPlatformSettings settings = GetPreferredPlatformSettings(platform);
                if (settings != null) platformSettings.Add(settings);
            }
            if (platformSettings.Count == 0) return false;
            return SetPlatformSettings(path, onPreimport, platformSettings.ToArray());
        }

        public static bool SetPlatformSettings(string path, Action<TextureImporterPlatformSettings, TextureImporterPlatformSettings> onPreimport = null, params TextureImporterPlatformSettings[] platformSettings)
        {
            bool modified = false;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) return modified;
            foreach (var settings in platformSettings)
            {
                TextureImporterPlatformSettings importerSettings = importer.GetPlatformTextureSettings(settings.name);
                TextureImporterPlatformSettings copySettings = new TextureImporterPlatformSettings();
                settings.CopyTo(copySettings);
                copySettings.maxTextureSize = Math.Min(copySettings.maxTextureSize, importerSettings.maxTextureSize);
                onPreimport?.Invoke(copySettings, importerSettings);
                if (!copySettings.VEqual(importerSettings))
                {
                    importer.SetPlatformTextureSettings(copySettings);
                    modified = true;
                }
            }
            if (modified) importer.SaveAndReimport();
            return modified;
        }

        private static TextureImporterSettings GetDefaultTextureSettings()
        {
            TextureImporterSettings settings = new TextureImporterSettings();
            s_DefaultSettings.CopyTo(settings);
            return settings;
        }

        public static TextureImporterSettings GetPreferredSpriteSettings()
        {
            TextureImporterSettings settings = new TextureImporterSettings();
            s_PreferredSpriteSettings.CopyTo(settings);
            return settings;
        }

        public static TextureImporterSettings GetPreferredTextureSettings()
        {
            TextureImporterSettings settings = new TextureImporterSettings();
            s_PreferredTextureSettings.CopyTo(settings);
            return settings;
        }

        public static bool SetDefaultTextureSettings(string path)
        {
            return SetTextureSettings(path, s_DefaultSettings);
        }

        public static bool SetPreferredSpriteSettings(string path, Action<TextureImporterSettings, TextureImporterSettings> onPreimport = null, PostImportTextureSettings onPostimport = null)
        {
            return SetTextureSettings(path, s_PreferredSpriteSettings, onPreimport, onPostimport);
        }

        public static bool SetPreferredTextureSettings(string path, Action<TextureImporterSettings, TextureImporterSettings> onPreimport = null, PostImportTextureSettings onPostimport = null)
        {
            return SetTextureSettings(path, s_PreferredTextureSettings, onPreimport, onPostimport);
        }

        public static bool SetTextureSettings(string path, TextureImporterSettings settings, Action<TextureImporterSettings, TextureImporterSettings> onPreimport = null, PostImportTextureSettings onPostimport = null)
        {
            bool modified = false;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) return modified;
            TextureImporterSettings importerSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(importerSettings);
            TextureImporterSettings copySettings = new TextureImporterSettings();
            settings.CopyTo(copySettings);
            onPreimport?.Invoke(copySettings, importerSettings);
            if (!copySettings.VEqual(importerSettings))
            {
                importer.SetTextureSettings(copySettings);
                modified = true;
            }
            if (onPostimport != null) modified = onPostimport.Invoke(importer) || modified;
            if (modified) importer.SaveAndReimport();
            return modified;
        }
    }
}
