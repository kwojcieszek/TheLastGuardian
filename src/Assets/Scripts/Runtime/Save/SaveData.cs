using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Campaign progress stored in the save file.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;
        public const int MaxPotions = 3;

        [SerializeField] private int version = CurrentVersion;
        [SerializeField] private int highestUnlockedLevel = 1;
        [SerializeField] private int lastPlayedLevel;
        [SerializeField] private int checkpointLevel;
        [SerializeField] private float checkpointX;
        [SerializeField] private int checkpointCoins;
        [SerializeField] private int checkpointChests;
        [SerializeField] private int potions;
        [SerializeField] private bool gameCompleted;
        [SerializeField] private string savedAt;
        [SerializeField] private List<LevelRecord> records = new List<LevelRecord>();

        public int Version => version;

        public int HighestUnlockedLevel => highestUnlockedLevel;

        public int LastPlayedLevel
        {
            get => lastPlayedLevel;
            set => lastPlayedLevel = value;
        }

        public int CheckpointLevel => checkpointLevel;

        public float CheckpointX => checkpointX;

        public int CheckpointCoins => checkpointCoins;

        public int CheckpointChests => checkpointChests;

        public int Potions
        {
            get => potions;
            set => potions = Mathf.Clamp(value, 0, MaxPotions);
        }

        public bool GameCompleted => gameCompleted;

        public string SavedAt => savedAt;

        public bool HasProgress => highestUnlockedLevel > 1 || lastPlayedLevel > 0 || checkpointLevel > 0;

        public int ContinueLevel => Mathf.Clamp(highestUnlockedLevel, 1, Mathf.Max(1, LevelCatalog.Count));

        public int TotalCoins
        {
            get
            {
                int total = 0;

                foreach (LevelRecord record in records)
                {
                    total += record.BestCoins;
                }

                return total;
            }
        }

        public int CompletedLevels
        {
            get
            {
                int count = 0;

                foreach (LevelRecord record in records)
                {
                    if (record.Completed)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public bool IsUnlocked(int levelId)
        {
            return levelId <= highestUnlockedLevel;
        }

        public LevelRecord FindRecord(int levelId)
        {
            return records.Find(record => record.LevelId == levelId);
        }

        public void SetCheckpoint(int levelId, float x, int coins, int chests)
        {
            checkpointLevel = levelId;
            checkpointX = x;
            checkpointCoins = coins;
            checkpointChests = chests;
            lastPlayedLevel = levelId;
        }

        public void ClearCheckpoint()
        {
            checkpointLevel = 0;
            checkpointX = 0f;
            checkpointCoins = 0;
            checkpointChests = 0;
        }

        public void CompleteLevel(int levelId, int coins, int chests, float timeSeconds, int levelCount)
        {
            LevelRecord record = FindRecord(levelId);

            if (record == null)
            {
                record = new LevelRecord(levelId);
                records.Add(record);
            }

            record.RegisterCompletion(coins, chests, timeSeconds);
            ClearCheckpoint();

            if (levelId >= levelCount)
            {
                gameCompleted = true;
                lastPlayedLevel = levelId;

                return;
            }

            highestUnlockedLevel = Mathf.Max(highestUnlockedLevel, levelId + 1);
            lastPlayedLevel = levelId + 1;
        }

        public void MarkSaved()
        {
            version = CurrentVersion;
            savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public void UnlockAllLevels(int levelCount)
        {
            highestUnlockedLevel = Mathf.Max(highestUnlockedLevel, levelCount);
        }
    }
}
