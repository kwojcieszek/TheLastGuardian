#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PiterGame.QA
{
    /// <summary>
    /// Plays through the menu and every level with simulated input, verifies saving and captures screenshots.
    /// </summary>
    public sealed class SmokeTestRunner : MonoBehaviour
    {
        private const int ShotWidth = 1600;
        private const int ShotHeight = 900;
        private const float LoadTimeout = 30f;

        private readonly List<string> errors = new List<string>();
        private readonly List<string> failures = new List<string>();
        private string outputFolder;
        private int projectilesSeen;

        public static string OutputFolder { get; set; } = "Logs/SmokeTest";

        public static Action<int> Finished { get; set; }

        private static bool HazardAhead(Vector2 position, float feet)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(new Vector2(position.x + 2f, feet + 0.4f), new Vector2(2.2f, 0.8f), 0f, 1 << GameLayers.Interactable);

            foreach (Collider2D hit in hits)
            {
                Hazard hazard = hit.GetComponent<Hazard>();

                if (hazard != null && !hazard.Lethal)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Teleport(PiterController player, Vector2 position)
        {
            player.Body.position = position;
            player.transform.position = position;
            player.Body.linearVelocity = Vector2.zero;
        }

        private IEnumerator Start()
        {
            Application.logMessageReceived += OnLog;
            outputFolder = OutputFolder;
            Directory.CreateDirectory(outputFolder);
            BackupSaves();

            yield return RunScenario();

            RestoreSaves();
            Application.logMessageReceived -= OnLog;

            foreach (string error in errors)
            {
                failures.Add("Logged error: " + error);
            }

            foreach (string failure in failures)
            {
                Debug.Log("SMOKE_FAIL: " + failure);
            }

            Debug.Log(failures.Count == 0 ? "SMOKE_TEST_PASSED" : $"SMOKE_TEST_FAILED ({failures.Count})");
            Finished?.Invoke(failures.Count == 0 ? 0 : 1);
        }

        private IEnumerator RunScenario()
        {
            yield return WaitUntilRealtime(() => FindFirstObjectByType<MainMenuController>() != null, "main menu");
            yield return new WaitForSecondsRealtime(2f);
            yield return Capture("01_main_menu");

            LevelSelectView levelSelect = FindFirstObjectByType<LevelSelectView>(FindObjectsInactive.Include);
            levelSelect.Show();

            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture("01b_level_select");

            levelSelect.Close();

            SettingsView settings = FindFirstObjectByType<SettingsView>(FindObjectsInactive.Include);
            settings.Show();

            yield return new WaitForSecondsRealtime(0.3f);
            yield return Capture("01c_settings");

            settings.Close();

            SaveSystem.ResetProgress();
            SceneFlow.StartNewGame();

            yield return WaitForLevel(1);
            yield return Capture("02_level01_title_card");

            LevelSession session = LevelSession.Current;
            session.SkipIntro();
            Check(session.State == LevelState.Playing, "level 1 starts playing after the intro");
            Check(!session.Player.AirJumpUnlocked, "double jump locked in level 1");

            yield return Drive(session, 1.6f, 1f);
            yield return Capture("02b_level01_story_scene");

            yield return Drive(session, 25f, 2f);

            StoryNpc[] storyCharacters = FindObjectsByType<StoryNpc>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Check(storyCharacters.Length == 2 && Array.TrueForAll(storyCharacters, npc => npc.HasSpoken), "level 1 abduction scene plays");
            yield return Capture("03_level01_gameplay");

            yield return TestCheckpointAndRespawn(session);
            yield return TestPause(session);
            yield return TestSaveAndContinue();
            yield return TestCompletion();
            yield return TestObjectiveLevel();
            yield return TestRemainingLevels();
            yield return TestFinalBoss();

            SceneFlow.LoadMainMenu();

            yield return WaitUntilRealtime(() => FindFirstObjectByType<MainMenuController>() != null, "menu after ending");
            yield return new WaitForSecondsRealtime(1f);
            yield return Capture("20_main_menu_after_ending");
        }

        private IEnumerator TestCheckpointAndRespawn(LevelSession session)
        {
            Checkpoint2D[] checkpoints = FindObjectsByType<Checkpoint2D>(FindObjectsSortMode.None);
            Array.Sort(checkpoints, (a, b) => a.SpawnPoint.x.CompareTo(b.SpawnPoint.x));
            Checkpoint2D target = checkpoints[1];

            Teleport(session.Player, target.SpawnPoint + new Vector2(0f, 0.3f));

            yield return new WaitForSeconds(0.6f);

            SaveData save = SaveSystem.Current;
            Check(save.CheckpointLevel == 1 && Mathf.Abs(save.CheckpointX - target.SpawnPoint.x) < 0.5f, "checkpoint saved to disk");
            Check(JsonFileStore.Exists(SaveSystem.FileName), "save file exists");

            int respawns = session.Respawns;
            Teleport(session.Player, new Vector2(target.SpawnPoint.x + 3f, -7f));

            yield return WaitUntilRealtime(() => session.Respawns > respawns, "respawn after falling");

            Check(Mathf.Abs(session.Player.transform.position.x - target.SpawnPoint.x) < 1.5f, "respawned at checkpoint");
            Check(session.Player.Health.CurrentHealth == session.Player.Health.MaxHealth, "respawn restores health");
        }

        private IEnumerator TestPause(LevelSession session)
        {
            session.Pause();

            yield return null;

            Check(session.State == LevelState.Paused && Mathf.Approximately(Time.timeScale, 0f), "pause stops time");

            yield return Capture("04_pause_menu");

            session.Resume();
            Check(session.State == LevelState.Playing && Mathf.Approximately(Time.timeScale, 1f), "resume restores time");
        }

        private IEnumerator TestSaveAndContinue()
        {
            float savedX = SaveSystem.Current.CheckpointX;
            LevelSession.Current.QuitToMenu();

            yield return WaitUntilRealtime(() => FindFirstObjectByType<MainMenuController>() != null, "menu after quit");
            yield return new WaitForSecondsRealtime(1f);
            yield return Capture("05_main_menu_continue");

            SaveSystem.Reload();
            Check(SaveSystem.Current.HasProgress && Mathf.Approximately(SaveSystem.Current.CheckpointX, savedX), "progress reloaded from disk");

            SceneFlow.ContinueGame();

            yield return WaitForLevel(1);

            LevelSession session = LevelSession.Current;
            Check(Mathf.Abs(session.SpawnPoint.x - savedX) < 0.1f, "continue resumes at the saved checkpoint");
            Check(Mathf.Abs(session.Player.transform.position.x - savedX) < 1f, "player placed at the saved checkpoint");

            session.SkipIntro();
        }

        private IEnumerator TestCompletion()
        {
            LevelSession session = LevelSession.Current;
            LevelExit exit = FindFirstObjectByType<LevelExit>();

            Teleport(session.Player, new Vector2(exit.transform.position.x, -0.6f));

            yield return WaitUntilRealtime(() => session.State == LevelState.Completed, "level 1 completion");
            yield return WaitUntilRealtime(() => session.Ui.LevelComplete.gameObject.activeSelf, "stage complete screen");
            yield return Capture("06_stage_complete");

            SaveData save = SaveSystem.Current;
            Check(save.HighestUnlockedLevel >= 2, "level 2 unlocked");
            Check(save.CheckpointLevel == 0, "checkpoint cleared after completion");
            Check(save.FindRecord(1) != null && save.FindRecord(1).Completed, "level 1 record stored");
        }

        private IEnumerator TestObjectiveLevel()
        {
            SceneFlow.LoadLevel(2, false);

            yield return WaitForLevel(2);

            LevelSession session = LevelSession.Current;
            session.SkipIntro();
            Check(session.Player.AirJumpUnlocked, "double jump unlocked in level 2");
            Check(session.ObjectiveTarget == 1, "level 2 objective configured");

            ObjectiveItem item = FindFirstObjectByType<ObjectiveItem>();
            Check(item != null, "objective item exists");

            if (item != null)
            {
                Teleport(session.Player, (Vector2)item.transform.position + new Vector2(-0.2f, 0.2f));

                yield return new WaitForSeconds(0.5f);

                Check(session.ObjectivesCollected == 1, "objective collected");
            }

            yield return Drive(session, 8f, 2f);
            yield return Capture("07_level02_forest");
        }

        private IEnumerator TestRemainingLevels()
        {
            for (int levelId = 3; levelId < LevelCatalog.Count; levelId++)
            {
                SceneFlow.LoadLevel(levelId, false);

                yield return WaitForLevel(levelId);

                LevelSession session = LevelSession.Current;
                LevelDefinition definition = session.Definition;
                Check(definition != null && definition.Id == levelId, $"level {levelId} definition loaded");
                Check(FindObjectsByType<Checkpoint2D>(FindObjectsSortMode.None).Length == 3, $"level {levelId} has three checkpoints");
                Check(FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length >= 8, $"level {levelId} has enemies");

                bool needsGuardian = levelId == 3 || levelId == 6 || levelId == 7;
                Check(!needsGuardian || session.Guardian != null, $"level {levelId} guardian present");
                Check(FindObjectsByType<ObjectiveItem>(FindObjectsSortMode.None).Length == session.ObjectiveTarget, $"level {levelId} objective count");

                session.SkipIntro();

                yield return Drive(session, 4f, 1f);
                yield return Capture($"{levelId + 5:00}_level{levelId:00}");
            }
        }

        private IEnumerator TestFinalBoss()
        {
            int finalLevel = LevelCatalog.Count;
            SceneFlow.LoadLevel(finalLevel, false);

            yield return WaitForLevel(finalLevel);

            LevelSession session = LevelSession.Current;
            session.SkipIntro();

            EnemyController boss = session.Guardian;
            Check(boss != null && boss.GetComponent<MalachorBoss>() != null, "Malachor present in the final level");

            if (boss == null)
            {
                yield break;
            }

            session.Player.Health.MaxHealth = 99;
            session.Player.Health.Restore();
            Teleport(session.Player, new Vector2(boss.transform.position.x - 9f, -0.6f));

            float end = Time.realtimeSinceStartup + 8f;

            while (Time.realtimeSinceStartup < end)
            {
                projectilesSeen = Mathf.Max(projectilesSeen, FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length);

                yield return null;
            }

            Check(boss.GetComponent<MalachorBoss>().IsActivated, "boss fight starts");
            Check(projectilesSeen > 0, "boss fires orbs");
            Check(session.BossEngaged, "boss bar shown");

            yield return Capture("15_final_boss");

            boss.Health.Kill();

            yield return new WaitForSeconds(1.2f);

            LevelExit exit = FindFirstObjectByType<LevelExit>();
            Teleport(session.Player, new Vector2(exit.transform.position.x, -0.6f));

            yield return WaitUntilRealtime(() => session.State == LevelState.Completed, "final level completion");

            float dialogueEnd = Time.realtimeSinceStartup + 20f;

            while (!session.Ui.Ending.gameObject.activeSelf && Time.realtimeSinceStartup < dialogueEnd)
            {
                yield return new WaitForSecondsRealtime(0.35f);

                session.Ui.Dialogue.Advance();
            }

            Check(session.Ui.Ending.gameObject.activeSelf, "ending screen shown");
            Check(SaveSystem.Current.GameCompleted, "game marked as completed");

            yield return new WaitForSecondsRealtime(1.5f);
            yield return Capture("16_ending");
        }

        private IEnumerator Drive(LevelSession session, float realtimeSeconds, float timeScale)
        {
            PiterController player = session.Player;
            PiterInput input = player.Input;
            float startX = player.transform.position.x;
            float maxX = startX;
            float lastJump = -10f;
            float end = Time.realtimeSinceStartup + realtimeSeconds;

            player.Health.MaxHealth = 99;
            player.Health.Restore();
            player.AirJumpUnlocked = true;
            Time.timeScale = timeScale;

            while (Time.realtimeSinceStartup < end)
            {
                if (player.IsDead || session.State != LevelState.Playing)
                {
                    yield return null;

                    continue;
                }

                input.SimulateHorizontal(1f);

                Vector2 position = player.Body.position;
                float feet = player.FeetY;
                bool gap = Physics2D.Raycast(new Vector2(position.x + 1.3f, feet + 0.2f), Vector2.down, 0.7f, GameLayers.GroundMask).collider == null;
                bool wall = Physics2D.BoxCast(position, new Vector2(0.35f, 1.1f), 0f, Vector2.right, 1.3f, GameLayers.GroundMask).collider != null;
                bool hazard = HazardAhead(position, feet);
                bool falling = !player.IsGrounded && player.Body.linearVelocity.y < -1f;
                bool noGroundBelow = Physics2D.Raycast(new Vector2(position.x + 0.6f, feet), Vector2.down, 3f, GameLayers.GroundMask).collider == null;

                if (player.IsGrounded && Time.time - lastJump > 0.3f && (gap || wall || hazard))
                {
                    input.SimulateJump();
                    lastJump = Time.time;
                }
                else if (falling && noGroundBelow && Time.time - lastJump > 0.25f)
                {
                    input.SimulateJump();
                    lastJump = Time.time;
                }

                if (Physics2D.OverlapCircle(position + new Vector2(1.2f, 0f), 0.9f, 1 << GameLayers.Enemy) != null)
                {
                    input.SimulateAttack();
                }

                maxX = Mathf.Max(maxX, player.transform.position.x);

                yield return new WaitForFixedUpdate();
            }

            input.StopSimulation();
            input.Clear();
            Time.timeScale = session.State == LevelState.Playing ? 1f : Time.timeScale;

            Debug.Log($"SMOKE_DRIVE level {session.LevelId}: from {startX:0.0} to max {maxX:0.0}, coins {session.Coins}, enemies {session.EnemiesDefeated}, respawns {session.Respawns}");
            Check(maxX - startX > Mathf.Min(15f, realtimeSeconds * timeScale * 2.5f), $"level {session.LevelId} traversal makes progress");
        }

        private IEnumerator WaitForLevel(int levelId)
        {
            yield return WaitUntilRealtime(() => LevelSession.Current != null && LevelSession.Current.LevelId == levelId && !ScreenFader.IsBusy, $"level {levelId} loaded");
            yield return null;
        }

        private IEnumerator WaitUntilRealtime(Func<bool> condition, string description)
        {
            float deadline = Time.realtimeSinceStartup + LoadTimeout;

            while (!condition())
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    failures.Add("Timeout waiting for " + description);

                    yield break;
                }

                yield return null;
            }
        }

        private IEnumerator Capture(string name)
        {
            Camera camera = Camera.main;

            if (camera == null)
            {
                failures.Add("No camera for screenshot " + name);

                yield break;
            }

            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            List<Canvas> switched = new List<Canvas>();

            foreach (Canvas canvas in canvases)
            {
                if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = 1f + (1000 - canvas.sortingOrder) * 0.001f;
                    switched.Add(canvas);
                }
            }

            RenderTexture target = new RenderTexture(ShotWidth, ShotHeight, 24);
            RenderTexture previousTarget = camera.targetTexture;
            camera.targetTexture = target;

            yield return null;

            Canvas.ForceUpdateCanvases();
            camera.Render();

            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = target;

            Texture2D image = new Texture2D(ShotWidth, ShotHeight, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, ShotWidth, ShotHeight), 0, 0);
            image.Apply();

            File.WriteAllBytes(Path.Combine(outputFolder, name + ".png"), image.EncodeToPNG());

            RenderTexture.active = previousActive;
            camera.targetTexture = previousTarget;

            foreach (Canvas canvas in switched)
            {
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }

            Destroy(image);
            target.Release();
            Destroy(target);
        }

        private void Check(bool condition, string description)
        {
            if (condition)
            {
                Debug.Log("SMOKE_OK: " + description);

                return;
            }

            failures.Add(description);
        }

        private void OnLog(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                errors.Add(message + "\n" + stackTrace);
            }
        }

        private void BackupSaves()
        {
            foreach (string file in new[] { SaveSystem.FileName, SettingsStore.FileName })
            {
                string path = JsonFileStore.GetPath(file);

                if (File.Exists(path))
                {
                    File.Copy(path, path + ".smoke", true);
                }
            }
        }

        private void RestoreSaves()
        {
            foreach (string file in new[] { SaveSystem.FileName, SettingsStore.FileName })
            {
                string path = JsonFileStore.GetPath(file);
                string backup = path + ".smoke";

                if (File.Exists(backup))
                {
                    File.Copy(backup, path, true);
                    File.Delete(backup);
                }
                else if (File.Exists(path) && file == SaveSystem.FileName)
                {
                    File.Delete(path);
                }
            }

            SaveSystem.Reload();
        }
    }
}
#endif
