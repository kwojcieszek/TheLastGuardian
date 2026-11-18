using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Persistent best results for one level.
    /// </summary>
    [Serializable]
    public sealed class LevelRecord
    {
        [SerializeField] private int levelId;
        [SerializeField] private bool completed;
        [SerializeField] private int bestCoins;
        [SerializeField] private int bestChests;
        [SerializeField] private float bestTimeSeconds;

        public int LevelId => levelId;

        public bool Completed => completed;

        public int BestCoins => bestCoins;

        public int BestChests => bestChests;

        public float BestTimeSeconds => bestTimeSeconds;

        public LevelRecord()
        {
        }

        public LevelRecord(int levelId)
        {
            this.levelId = levelId;
        }

        public void RegisterCompletion(int coins, int chests, float timeSeconds)
        {
            completed = true;
            bestCoins = Mathf.Max(bestCoins, coins);
            bestChests = Mathf.Max(bestChests, chests);

            if (bestTimeSeconds <= 0f || timeSeconds < bestTimeSeconds)
            {
                bestTimeSeconds = timeSeconds;
            }
        }
    }
}
