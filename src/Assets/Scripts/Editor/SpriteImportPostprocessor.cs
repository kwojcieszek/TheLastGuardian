using UnityEditor;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Applies the sprite import rules to every PNG under the game art folder.
    /// </summary>
    public sealed class SpriteImportPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (SpriteImportRules.IsGameSprite(assetPath))
            {
                SpriteImportRules.Configure((TextureImporter)assetImporter, assetPath);
            }
        }
    }
}
