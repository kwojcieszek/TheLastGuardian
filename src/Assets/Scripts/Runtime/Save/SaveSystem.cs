namespace PiterGame
{
    /// <summary>
    /// Loads, updates and persists campaign progress.
    /// </summary>
    public static class SaveSystem
    {
        public const string FileName = "piter_save.json";

        private static SaveData current;

        public static SaveData Current
        {
            get
            {
                if (current == null)
                {
                    current = JsonFileStore.Read<SaveData>(FileName) ?? new SaveData();
                }

                return current;
            }
        }

        public static bool HasProgress => Current.HasProgress;

        public static bool Save()
        {
            Current.MarkSaved();

            return JsonFileStore.Write(FileName, Current);
        }

        public static void ResetProgress()
        {
            JsonFileStore.Delete(FileName);
            current = new SaveData();

            Save();
        }

        public static void Reload()
        {
            current = null;
        }

        public static void RecordCheckpoint(int levelId, float x, int coins, int chests, int potions)
        {
            Current.SetCheckpoint(levelId, x, coins, chests);
            Current.Potions = potions;

            Save();
        }

        public static void RecordLevelStarted(int levelId, int potions)
        {
            Current.LastPlayedLevel = levelId;
            Current.Potions = potions;

            Save();
        }

        public static void RecordLevelComplete(int levelId, int coins, int chests, float timeSeconds, int potions)
        {
            Current.CompleteLevel(levelId, coins, chests, timeSeconds, LevelCatalog.Count);
            Current.Potions = potions;

            Save();
        }
    }
}
