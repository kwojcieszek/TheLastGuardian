using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Central place for switching between the main menu and level scenes.
    /// </summary>
    public static class SceneFlow
    {
        public const string MainMenuScene = "MainMenu";

        private static LevelLaunchRequest pendingRequest;

        public static void LoadMainMenu()
        {
            Time.timeScale = 1f;
            TouchInputState.Reset();

            ScreenFader.LoadScene(MainMenuScene);
        }

        public static void LoadLevel(int levelId, bool resumeFromCheckpoint)
        {
            LevelDefinition level = LevelCatalog.Get(levelId);

            if (level == null)
            {
                Debug.LogError("Unknown level id: " + levelId);

                return;
            }

            pendingRequest = new LevelLaunchRequest(levelId, resumeFromCheckpoint);
            Time.timeScale = 1f;
            TouchInputState.Reset();

            ScreenFader.LoadScene(level.Scene);
        }

        public static void StartNewGame()
        {
            SaveSystem.ResetProgress();

            LoadLevel(1, false);
        }

        public static void ContinueGame()
        {
            SaveData save = SaveSystem.Current;

            if (save.CheckpointLevel > 0)
            {
                LoadLevel(save.CheckpointLevel, true);

                return;
            }

            LoadLevel(save.ContinueLevel, false);
        }

        public static LevelLaunchRequest ConsumeRequest(int levelId)
        {
            LevelLaunchRequest request = pendingRequest;
            pendingRequest = null;

            if (request == null || request.LevelId != levelId)
            {
                return new LevelLaunchRequest(levelId, false);
            }

            return request;
        }
    }
}
