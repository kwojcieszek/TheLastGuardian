using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Read-only access to the campaign level definitions.
    /// </summary>
    public static class LevelCatalog
    {
        private const string ResourcePath = "Data/levels";

        private static LevelDefinition[] levels;

        public static IReadOnlyList<LevelDefinition> Levels => GetLevels();

        public static int Count => GetLevels().Length;

        public static LevelDefinition Get(int levelId)
        {
            LevelDefinition[] all = GetLevels();

            return Array.Find(all, level => level.Id == levelId);
        }

        public static bool IsFinalLevel(int levelId)
        {
            return levelId >= Count;
        }

        private static LevelDefinition[] GetLevels()
        {
            if (levels != null)
            {
                return levels;
            }

            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);

            if (asset == null)
            {
                Debug.LogError("Missing level catalog: Resources/" + ResourcePath);
                levels = Array.Empty<LevelDefinition>();

                return levels;
            }

            LevelCatalogData data = JsonUtility.FromJson<LevelCatalogData>(asset.text);
            levels = data != null ? data.Levels : Array.Empty<LevelDefinition>();

            return levels;
        }
    }
}
