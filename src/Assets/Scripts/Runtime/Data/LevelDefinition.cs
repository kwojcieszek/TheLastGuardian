using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Story and design data of a single campaign level, loaded from levels.json.
    /// </summary>
    [Serializable]
    public sealed class LevelDefinition
    {
        [SerializeField] private int id;
        [SerializeField] private string slug;
        [SerializeField] private string name;
        [SerializeField] private string biome;
        [SerializeField] private int lengthMeters;
        [SerializeField] private int difficulty;
        [SerializeField] private string mechanic;
        [SerializeField] private string[] enemyTypes;
        [SerializeField] private string objective;
        [SerializeField] private string story;
        [SerializeField] private string scene;

        public int Id => id;

        public string Slug => slug;

        public string Name => name;

        public string Biome => biome;

        public int LengthMeters => lengthMeters;

        public int Difficulty => difficulty;

        public string Mechanic => mechanic;

        public string[] EnemyTypes => enemyTypes ?? Array.Empty<string>();

        public string Objective => objective;

        public string Story => story;

        public string Scene => scene;

        public string Title => $"{id:00}. {name}";
    }
}
