using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PiterGame
{
    /// <summary>
    /// Runs a level: intro, checkpoints, respawning, collectibles, objectives, pause, completion and saving.
    /// </summary>
    public sealed class LevelSession : MonoBehaviour
    {
        private const float RespawnDelay = 1.6f;
        private const float BlockedMessageCooldown = 2f;
        private const float BossEngageDistance = 16f;
        private const int FinalLevelIntroLines = 2;

        private static LevelSession current;

        private LevelUi ui;
        private LevelDefinition definition;
        private Checkpoint2D[] checkpoints = new Checkpoint2D[0];
        private Vector2 spawnPoint;
        private float lastBlockedMessageTime = -100f;

        [field: SerializeField] public int LevelId { get; set; } = 1;

        [field: SerializeField] public float LevelLength { get; set; } = 1000f;

        [field: SerializeField] public PiterController Player { get; set; }

        [field: SerializeField] public FollowCamera2D FollowCamera { get; set; }

        [field: SerializeField] public Vector2 StartPoint { get; set; } = new Vector2(4f, -0.65f);

        [field: SerializeField] public EnemyController Guardian { get; set; }

        [field: SerializeField] public int ObjectiveTarget { get; set; }

        [field: SerializeField] public string ObjectiveLabel { get; set; } = string.Empty;

        [field: SerializeField] public int TotalCoins { get; set; }

        [field: SerializeField] public int TotalChests { get; set; }

        public static LevelSession Current => current;

        public LevelState State { get; private set; } = LevelState.Intro;

        public LevelDefinition Definition => definition;

        public int Coins { get; private set; }

        public int Potions { get; private set; }

        public int ObjectivesCollected { get; private set; }

        public int ChestsFound { get; private set; }

        public int EnemiesDefeated { get; private set; }

        public int Respawns { get; private set; }

        public float ElapsedTime { get; private set; }

        public bool BossEngaged { get; private set; }

        public Vector2 SpawnPoint => spawnPoint;

        public LevelUi Ui => ui;

        public string LevelTitle => definition != null ? definition.Title : "Stage " + LevelId;

        public float Progress => Player != null && LevelLength > 0f ? Mathf.Clamp01(Player.transform.position.x / LevelLength) : 0f;

        public bool ObjectivesComplete => ObjectivesCollected >= ObjectiveTarget;

        public bool GuardianDefeated => Guardian == null || Guardian.IsDead;

        public string ObjectiveStatus
        {
            get
            {
                string status = string.Empty;

                if (ObjectiveTarget > 0)
                {
                    status = $"{ObjectiveLabel}: {Mathf.Min(ObjectivesCollected, ObjectiveTarget)} / {ObjectiveTarget}";
                }

                if (Guardian != null && !Guardian.IsDead)
                {
                    status += (status.Length > 0 ? "     " : string.Empty) + "Defeat " + Guardian.DisplayName;
                }

                if (status.Length == 0)
                {
                    status = "Reach the exit flag";
                }

                return status;
            }
        }

        public void ShowToast(string message)
        {
            if (ui != null)
            {
                ui.Hud.ShowToast(message);
            }
        }

        public void ActivateCheckpoint(Checkpoint2D checkpoint)
        {
            checkpoint.MarkActivated();

            if (checkpoint.SpawnPoint.x <= spawnPoint.x + 0.1f || State != LevelState.Playing)
            {
                return;
            }

            spawnPoint = checkpoint.SpawnPoint;
            Player.Health.Heal(Player.Health.MaxHealth);

            SaveSystem.RecordCheckpoint(LevelId, spawnPoint.x, Coins, ChestsFound, Potions);
            GameAudio.Play(SoundEffect.Checkpoint);
            ShowToast("Checkpoint reached - game saved");
        }

        public void CollectCoin(int value)
        {
            Coins += value;
        }

        public bool TryCollectPotion()
        {
            Health health = Player.Health;

            if (health.CurrentHealth < health.MaxHealth)
            {
                health.Heal(1);
                ShowToast("+1 heart");

                return true;
            }

            if (Potions < SaveData.MaxPotions)
            {
                Potions++;
                ShowToast("Potion stored - press E (or the potion button) to drink");

                return true;
            }

            return false;
        }

        public void OpenChest(int coinReward)
        {
            ChestsFound++;
            Coins += coinReward;

            string message = $"Secret chest! +{coinReward} coins";

            if (Potions < SaveData.MaxPotions)
            {
                Potions++;
                message += " and a potion";
            }

            ShowToast(message);
        }

        public void CollectObjective()
        {
            ObjectivesCollected++;

            if (ObjectivesComplete)
            {
                string next = GuardianDefeated ? "Head to the exit!" : "Now defeat " + Guardian.DisplayName + "!";
                ShowToast($"{ObjectiveLabel} complete! {next}");

                return;
            }

            ShowToast($"{ObjectiveLabel}: {ObjectivesCollected} / {ObjectiveTarget}");
        }

        public void NotifyEnemyDefeated(EnemyController enemy)
        {
            EnemiesDefeated++;

            if (enemy != Guardian)
            {
                return;
            }

            if (LevelCatalog.IsFinalLevel(LevelId))
            {
                ShowToast("Malachor is defeated! Free Aleksandra!");

                return;
            }

            string next = ObjectivesComplete ? "The way is open!" : "Complete the objective to open the way.";
            ShowToast($"{enemy.DisplayName} defeated! {next}");
        }

        public void TryCompleteLevel()
        {
            if (State != LevelState.Playing || Player.IsDead)
            {
                return;
            }

            if (!GuardianDefeated)
            {
                ShowBlockedMessage($"Defeat {Guardian.DisplayName} first!");

                return;
            }

            if (!ObjectivesComplete)
            {
                ShowBlockedMessage($"{ObjectiveLabel}: {ObjectivesCollected} / {ObjectiveTarget} - the way is still sealed");

                return;
            }

            CompleteLevel();
        }

        public void Pause()
        {
            if (State != LevelState.Playing)
            {
                return;
            }

            State = LevelState.Paused;
            Time.timeScale = 0f;
            Player.Input.InputEnabled = false;

            ui.Pause.Show($"{LevelTitle}\nCoins: {Coins}   Time: {UiFactory.FormatTime(ElapsedTime)}");
        }

        public void Resume()
        {
            if (State != LevelState.Paused)
            {
                return;
            }

            ui.Settings.Close();
            ui.Pause.Hide();

            State = LevelState.Playing;
            Time.timeScale = 1f;
            Player.Input.InputEnabled = true;
        }

        public void RestartFromCheckpoint()
        {
            SaveData save = SaveSystem.Current;
            bool hasCheckpoint = save.CheckpointLevel == LevelId;

            SceneFlow.LoadLevel(LevelId, hasCheckpoint);
        }

        public void RestartLevel()
        {
            SceneFlow.LoadLevel(LevelId, false);
        }

        public void QuitToMenu()
        {
            SaveSystem.Current.Potions = Potions;
            SaveSystem.Save();

            SceneFlow.LoadMainMenu();
        }

        public void SkipIntro()
        {
            if (State == LevelState.Intro)
            {
                ui.Dialogue.gameObject.SetActive(false);
                StartPlaying();
            }
        }

        private static bool PausePressed()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;

            bool keyboardPressed = keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame);
            bool gamepadPressed = gamepad != null && gamepad.startButton.wasPressedThisFrame;

            return keyboardPressed || gamepadPressed;
        }

        private static void ClearProjectiles()
        {
            foreach (Projectile projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
            {
                Destroy(projectile.gameObject);
            }
        }

        private void Awake()
        {
            current = this;
            Time.timeScale = 1f;

            GameLayers.ConfigureCollisions();
            GameAudio.EnsureInstance();
            SettingsStore.Apply();

            definition = LevelCatalog.Get(LevelId);
            ui = new LevelUi(this);

            ui.Hud.PauseRequested += Pause;
            ui.Pause.ResumeRequested += Resume;
            ui.Pause.RestartCheckpointRequested += RestartFromCheckpoint;
            ui.Pause.RestartLevelRequested += RestartLevel;
            ui.Pause.SettingsRequested += OpenSettings;
            ui.Pause.MainMenuRequested += QuitToMenu;
            ui.Settings.Closed += OnSettingsClosed;
            ui.LevelComplete.NextRequested += LoadNextLevel;
            ui.LevelComplete.ReplayRequested += RestartLevel;
            ui.LevelComplete.MainMenuRequested += SceneFlow.LoadMainMenu;
            ui.Ending.MainMenuRequested += SceneFlow.LoadMainMenu;
        }

        private void Start()
        {
            LevelLaunchRequest request = SceneFlow.ConsumeRequest(LevelId);
            SaveData save = SaveSystem.Current;
            bool resumed = request.ResumeFromCheckpoint && save.CheckpointLevel == LevelId;

            checkpoints = FindObjectsByType<Checkpoint2D>(FindObjectsSortMode.None);
            spawnPoint = StartPoint;
            Potions = save.Potions;

            Player.AirJumpUnlocked = LevelId >= 2;
            Player.Health.Died += OnPlayerDied;

            if (resumed)
            {
                RestoreCheckpoint(save);
            }

            SaveSystem.RecordLevelStarted(LevelId, Potions);

            FollowCamera.Target = Player.transform;
            FollowCamera.SnapToTarget();

            BeginIntro(resumed);
        }

        private void OnDestroy()
        {
            if (current == this)
            {
                current = null;
            }

            if (Player != null)
            {
                Player.Health.Died -= OnPlayerDied;
            }

            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (State == LevelState.Playing)
            {
                ElapsedTime += Time.deltaTime;

                if (Player.Input.ConsumePotion())
                {
                    TryUsePotion();
                }

                UpdateBossEngagement();
            }

            if (PausePressed())
            {
                HandlePauseKey();
            }
        }

        private void RestoreCheckpoint(SaveData save)
        {
            spawnPoint = new Vector2(save.CheckpointX, StartPoint.y);
            Coins = save.CheckpointCoins;
            ChestsFound = save.CheckpointChests;

            foreach (Checkpoint2D checkpoint in checkpoints)
            {
                if (checkpoint.SpawnPoint.x <= spawnPoint.x + 0.1f)
                {
                    checkpoint.MarkActivated();
                }
            }

            DisablePickupsBehind(spawnPoint.x);
            Player.Respawn(spawnPoint);
        }

        private void DisablePickupsBehind(float x)
        {
            foreach (CoinPickup coin in FindObjectsByType<CoinPickup>(FindObjectsSortMode.None))
            {
                if (coin.transform.position.x < x)
                {
                    coin.gameObject.SetActive(false);
                }
            }

            foreach (TreasureChest chest in FindObjectsByType<TreasureChest>(FindObjectsSortMode.None))
            {
                if (chest.transform.position.x < x)
                {
                    chest.MarkOpened();
                }
            }
        }

        private void BeginIntro(bool resumed)
        {
            State = LevelState.Intro;
            Time.timeScale = 0f;
            Player.Input.InputEnabled = false;

            string subtitle = definition != null ? definition.Story : string.Empty;
            string body = BuildBriefing(resumed);
            string title = LevelTitle;

            if (resumed)
            {
                ui.Dialogue.ShowTitle(title, "Checkpoint restored", body, StartPlaying);

                return;
            }

            DialogueLine[] lines = SettingsStore.Current.SkipSeenDialogues ? new DialogueLine[0] : DialogueCatalog.GetIntroLines(LevelId, FinalLevelIntroLines);
            ui.Dialogue.ShowTitle(title, subtitle, body, () => ui.Dialogue.ShowLines(lines, StartPlaying));
        }

        private string BuildBriefing(bool resumed)
        {
            string objective = definition != null ? "Objective: " + definition.Objective : string.Empty;

            if (resumed)
            {
                return objective;
            }

            if (LevelId == 1)
            {
                return objective + "\n\nMove: A/D   Jump: Space   Attack: J   Potion: E   Pause: Esc\nStomp enemies by jumping on them!";
            }

            if (LevelId == 2)
            {
                return objective + "\n\nNew ability: DOUBLE JUMP - press jump again in the air!";
            }

            return objective;
        }

        private void StartPlaying()
        {
            State = LevelState.Playing;
            Time.timeScale = 1f;
            Player.Input.InputEnabled = true;
        }

        private void HandlePauseKey()
        {
            if (State == LevelState.Paused)
            {
                if (ui.Settings.IsOpen)
                {
                    ui.Settings.Close();

                    return;
                }

                Resume();

                return;
            }

            Pause();
        }

        private void OpenSettings()
        {
            ui.Pause.Hide();
            ui.Settings.Show();
        }

        private void OnSettingsClosed()
        {
            if (State == LevelState.Paused)
            {
                ui.Pause.Show($"{LevelTitle}\nCoins: {Coins}   Time: {UiFactory.FormatTime(ElapsedTime)}");
            }
        }

        private void TryUsePotion()
        {
            Health health = Player.Health;

            if (Potions <= 0)
            {
                ShowToast("No potions left");

                return;
            }

            if (health.CurrentHealth >= health.MaxHealth)
            {
                ShowToast("Your health is already full");

                return;
            }

            Potions--;
            health.Heal(1);
            GameAudio.Play(SoundEffect.Potion);
        }

        private void UpdateBossEngagement()
        {
            if (BossEngaged || Guardian == null || Guardian.IsDead)
            {
                return;
            }

            if (Mathf.Abs(Guardian.transform.position.x - Player.transform.position.x) < BossEngageDistance)
            {
                BossEngaged = true;
                ShowToast(Guardian.DisplayName + " blocks the way!");
            }
        }

        private void ShowBlockedMessage(string message)
        {
            if (Time.time - lastBlockedMessageTime < BlockedMessageCooldown)
            {
                return;
            }

            lastBlockedMessageTime = Time.time;
            GameAudio.Play(SoundEffect.Blocked);
            ShowToast(message);
        }

        private void CompleteLevel()
        {
            State = LevelState.Completed;
            Player.Freeze();
            Player.Input.InputEnabled = false;

            SaveSystem.RecordLevelComplete(LevelId, Coins, ChestsFound, ElapsedTime, Potions);
            ClearProjectiles();
            GameAudio.Play(SoundEffect.StageComplete);

            if (LevelCatalog.IsFinalLevel(LevelId))
            {
                Time.timeScale = 0f;
                DialogueLine[] outro = DialogueCatalog.GetOutroLines(LevelId, FinalLevelIntroLines);
                ui.Dialogue.ShowLines(outro, ShowEnding);

                return;
            }

            StartCoroutine(ShowResultsAfterDelay());
        }

        private IEnumerator ShowResultsAfterDelay()
        {
            yield return new WaitForSeconds(0.8f);

            Time.timeScale = 0f;
            bool hasNext = LevelCatalog.Get(LevelId + 1) != null;
            LevelCompletionSummary summary = new LevelCompletionSummary(LevelTitle, Coins, TotalCoins, ChestsFound, TotalChests, EnemiesDefeated, ElapsedTime, hasNext);

            ui.LevelComplete.Show(summary);
        }

        private void ShowEnding()
        {
            SaveData save = SaveSystem.Current;

            ui.Ending.Show(save.TotalCoins, save.CompletedLevels);
        }

        private void LoadNextLevel()
        {
            SceneFlow.LoadLevel(LevelId + 1, false);
        }

        private void OnPlayerDied()
        {
            if (State != LevelState.Playing)
            {
                return;
            }

            State = LevelState.Dying;
            Player.Input.InputEnabled = false;

            StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            ShowToast("Piter fell... returning to the checkpoint");

            yield return new WaitForSeconds(RespawnDelay);

            Player.Respawn(spawnPoint);
            FollowCamera.SnapToTarget();
            Respawns++;

            State = LevelState.Playing;
            Player.Input.InputEnabled = true;
        }
    }
}
