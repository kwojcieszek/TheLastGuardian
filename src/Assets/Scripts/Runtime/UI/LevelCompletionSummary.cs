namespace PiterGame
{
    /// <summary>
    /// Results displayed on the stage complete screen.
    /// </summary>
    public sealed class LevelCompletionSummary
    {
        public string LevelTitle { get; }

        public int Coins { get; }

        public int TotalCoins { get; }

        public int Chests { get; }

        public int TotalChests { get; }

        public int EnemiesDefeated { get; }

        public float TimeSeconds { get; }

        public bool HasNextLevel { get; }

        public LevelCompletionSummary(string levelTitle, int coins, int totalCoins, int chests, int totalChests, int enemiesDefeated, float timeSeconds, bool hasNextLevel)
        {
            LevelTitle = levelTitle;
            Coins = coins;
            TotalCoins = totalCoins;
            Chests = chests;
            TotalChests = totalChests;
            EnemiesDefeated = enemiesDefeated;
            TimeSeconds = timeSeconds;
            HasNextLevel = hasNextLevel;
        }
    }
}
