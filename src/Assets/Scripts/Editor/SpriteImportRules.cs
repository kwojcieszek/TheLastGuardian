using System;
using UnityEditor;
using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Texture import settings for game sprites (pixels per unit, wrap mode, compression).
    /// </summary>
    public static class SpriteImportRules
    {
        private const float TilePixelsPerUnit = 256f;
        private const float BackgroundPixelsPerUnit = 96f;
        private const float PropPixelsPerUnit = 200f;

        public static bool IsGameSprite(string assetPath)
        {
            return assetPath.StartsWith(GameAssetPaths.ArtRoot, StringComparison.Ordinal) && assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        }

        public static void Configure(TextureImporter importer, string assetPath)
        {
            string actor = ActorCatalog.ActorForPath(assetPath);
            bool tile = assetPath.Contains("/Tiles/");
            bool background = assetPath.Contains("/Backgrounds/");

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = actor != null ? ActorCatalog.Get(actor).PixelsPerUnit : tile ? TilePixelsPerUnit : background ? BackgroundPixelsPerUnit : PropPixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = tile ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = false;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        }
    }
}
