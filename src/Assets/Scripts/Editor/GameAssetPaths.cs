namespace PiterGame.EditorTools
{
    /// <summary>
    /// Asset locations used by the editor build tools.
    /// </summary>
    public static class GameAssetPaths
    {
        public const string ArtRoot = "Assets/Art/Sprites/";
        public const string PrefabRoot = "Assets/Prefabs/";
        public const string CharacterPrefabs = PrefabRoot + "Characters/";
        public const string GameplayPrefabs = PrefabRoot + "Gameplay/";
        public const string ScenesRoot = "Assets/Scenes/";
        public const string LevelScenesRoot = ScenesRoot + "Levels/";
        public const string MainMenuScene = ScenesRoot + "MainMenu.unity";
        public const string ArtLibraryAsset = "Assets/Resources/UiArtLibrary.asset";
        public const string LevelLayoutsJson = "Assets/Art/level_layouts.json";

        public static string Sprite(string relativePath)
        {
            return ArtRoot + relativePath + ".png";
        }

        public static string AnimationFrame(string actor, string state, int frame)
        {
            return $"{ArtRoot}Animations/{actor}/{state}_{frame:00}.png";
        }

        public static string BackgroundLayer(int levelIndex, string biome, int layer)
        {
            return $"{ArtRoot}Backgrounds/{levelIndex + 1:00}_{biome}/Parallax_{layer}.png";
        }

        public static string BackgroundPreview(int levelIndex, string biome)
        {
            return $"{ArtRoot}Backgrounds/{levelIndex + 1:00}_{biome}/Preview.png";
        }
    }
}
