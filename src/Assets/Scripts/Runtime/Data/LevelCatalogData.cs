using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Root object of levels.json.
    /// </summary>
    [Serializable]
    public sealed class LevelCatalogData
    {
        [SerializeField] private string game;
        [SerializeField] private LevelDefinition[] levels;

        public string Game => game;

        public LevelDefinition[] Levels => levels ?? Array.Empty<LevelDefinition>();
    }
}
