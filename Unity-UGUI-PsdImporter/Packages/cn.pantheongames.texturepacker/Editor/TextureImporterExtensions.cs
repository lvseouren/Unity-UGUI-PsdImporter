using UnityEditor;

namespace PantheonGamesEditor.TexturePacker
{
    internal static class TextureImporterExtensions
    {
        public static bool VEqual(this TextureImporterPlatformSettings settings, TextureImporterPlatformSettings otherSettings)
        {
            return settings.name == otherSettings.name
                && settings.overridden == otherSettings.overridden
                && settings.maxTextureSize == otherSettings.maxTextureSize
                && settings.resizeAlgorithm == otherSettings.resizeAlgorithm
                && settings.format == otherSettings.format
                && settings.textureCompression == otherSettings.textureCompression
                && settings.compressionQuality == otherSettings.compressionQuality
                && settings.crunchedCompression == otherSettings.crunchedCompression
                && settings.allowsAlphaSplitting == otherSettings.allowsAlphaSplitting
                && settings.androidETC2FallbackOverride == otherSettings.androidETC2FallbackOverride;
        }

        public static bool VEqual(this TextureImporterSettings settings, TextureImporterSettings otherSettings)
        {
            return settings.alphaIsTransparency == otherSettings.alphaIsTransparency
                && settings.alphaSource == otherSettings.alphaSource
                && settings.alphaTestReferenceValue == otherSettings.alphaTestReferenceValue
                && settings.aniso == otherSettings.aniso
                && settings.borderMipmap == otherSettings.borderMipmap
                && settings.convertToNormalMap == otherSettings.convertToNormalMap
                && settings.cubemapConvolution == otherSettings.cubemapConvolution
                && settings.fadeOut == otherSettings.fadeOut
                && settings.filterMode == otherSettings.filterMode
                && settings.generateCubemap == otherSettings.generateCubemap
                && settings.heightmapScale == otherSettings.heightmapScale
                && settings.mipMapsPreserveCoverage == otherSettings.mipMapsPreserveCoverage
                && settings.mipmapBias == otherSettings.mipmapBias
                && settings.mipmapEnabled == otherSettings.mipmapEnabled
                && settings.mipmapFadeDistanceEnd == otherSettings.mipmapFadeDistanceEnd
                && settings.mipmapFadeDistanceStart == otherSettings.mipmapFadeDistanceStart
                && settings.mipmapFilter == otherSettings.mipmapFilter
                && settings.normalMapFilter == otherSettings.normalMapFilter
                && settings.npotScale == otherSettings.npotScale
                && settings.readable == otherSettings.readable
                && settings.sRGBTexture == otherSettings.sRGBTexture
                && settings.seamlessCubemap == otherSettings.seamlessCubemap
                && settings.spriteAlignment == otherSettings.spriteAlignment
                && settings.spriteBorder == otherSettings.spriteBorder
                && settings.spriteExtrude == otherSettings.spriteExtrude
                && settings.spriteGenerateFallbackPhysicsShape == otherSettings.spriteGenerateFallbackPhysicsShape
                && settings.spriteMeshType == otherSettings.spriteMeshType
                && settings.spriteMode == otherSettings.spriteMode
                && settings.spritePivot == otherSettings.spritePivot
                && settings.spritePixelsPerUnit == otherSettings.spritePixelsPerUnit
                && settings.spriteTessellationDetail == otherSettings.spriteTessellationDetail
                && settings.textureShape == otherSettings.textureShape
                && settings.textureType == otherSettings.textureType
                && settings.wrapMode == otherSettings.wrapMode
                && settings.wrapModeU == otherSettings.wrapModeU
                && settings.wrapModeV == otherSettings.wrapModeV
                && settings.wrapModeW == otherSettings.wrapModeW;
        }
    }
}
