using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Title screen: continue, new game, stage select, settings, story and quit.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private const float BackgroundCycleSeconds = 7f;
        private const float CrossfadeSeconds = 1.6f;
        private const float BackgroundZoom = 0.04f;

        private static readonly Vector2 ButtonSize = new Vector2(560f, 84f);

        private Image backgroundFront;
        private Image backgroundBack;
        private RectTransform mainPanel;
        private Button continueButton;
        private Button newGameButton;
        private Text footerText;
        private LevelSelectView levelSelect;
        private SettingsView settings;
        private AboutView about;
        private ConfirmView confirm;
        private int previewIndex;
        private float nextCycleTime;
        private float crossfadeStart = -100f;

        private static bool BackPressed()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;

            return (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) || (gamepad != null && gamepad.buttonEast.wasPressedThisFrame);
        }

        private static Image CreateBackgroundImage(Transform root, string name)
        {
            RectTransform holder = UiFactory.CreateRect(name, root);
            UiFactory.Stretch(holder);

            Image image = holder.gameObject.AddComponent<Image>();
            image.preserveAspect = false;
            image.raycastTarget = false;

            AspectRatioFitter fitter = holder.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 16f / 9f;

            return image;
        }

        private static string DescribeContinue(SaveData save)
        {
            int levelId = save.CheckpointLevel > 0 ? save.CheckpointLevel : save.ContinueLevel;

            return $"Continue  -  Stage {levelId}";
        }

        private static void AddCharacter(Transform stage, UiArtLibrary art, string character, Vector2 anchor, Vector2 position, float size, bool facingLeft, Color tint, float offset)
        {
            Image image = UiFactory.CreateImage(stage, character, art.GetFace(character), new Vector2(size, size));
            UiFactory.Place(image.rectTransform, anchor, new Vector2(0.5f, 0f), position, new Vector2(size, size));
            image.rectTransform.localScale = new Vector3(facingLeft ? -1f : 1f, 1f, 1f);

            UiSpriteAnimator animator = image.gameObject.AddComponent<UiSpriteAnimator>();
            animator.Play(art.GetIdleFrames(character), 8f, offset);
            image.color = tint;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            TouchInputState.Reset();
            GameAudio.EnsureInstance();
            SettingsStore.Apply();
            UiFactory.EnsureEventSystem();

            Build();
            ShowMain();
        }

        private void Update()
        {
            UpdateBackground();

            if (BackPressed())
            {
                HandleBack();
            }
        }

        private void Build()
        {
            Canvas canvas = UiFactory.CreateCanvas("MainMenuUI", 10);
            Transform root = canvas.transform;

            BuildBackground(root);
            BuildTitle(root);
            BuildMainPanel(root);

            levelSelect = LevelSelectView.Create(root);
            levelSelect.LevelChosen += OnLevelChosen;
            levelSelect.Closed += ShowMain;

            settings = SettingsView.Create(root);
            settings.Closed += ShowMain;

            about = AboutView.Create(root);
            about.Closed += ShowMain;

            confirm = ConfirmView.Create(root);
            confirm.Closed += ShowMain;
        }

        private void BuildBackground(Transform root)
        {
            backgroundBack = CreateBackgroundImage(root, "BackgroundBack");
            backgroundFront = CreateBackgroundImage(root, "BackgroundFront");

            Image shade = UiFactory.CreatePanel(root, "Shade", new Color(0f, 0f, 0f, 0.3f));
            UiFactory.Stretch(shade.rectTransform);

            Image groundShade = UiFactory.CreatePanel(root, "GroundShade", new Color(0f, 0f, 0f, 0.35f));
            groundShade.rectTransform.anchorMin = new Vector2(0f, 0f);
            groundShade.rectTransform.anchorMax = new Vector2(1f, 0f);
            groundShade.rectTransform.pivot = new Vector2(0.5f, 0f);
            groundShade.rectTransform.sizeDelta = new Vector2(0f, 120f);
            groundShade.rectTransform.anchoredPosition = Vector2.zero;

            UiArtLibrary art = UiArtLibrary.Instance;

            BuildCharacterShowcase(root, art);

            previewIndex = 0;
            backgroundFront.sprite = art.GetLevelPreview(1);
            backgroundBack.sprite = backgroundFront.sprite;
            nextCycleTime = Time.unscaledTime + BackgroundCycleSeconds;
        }

        private void BuildTitle(Transform root)
        {
            Image banner = UiFactory.CreateFramedPanel(root, "TitleBanner", new Vector2(660f, 230f));
            UiFactory.Place(banner.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70f, -36f), new Vector2(660f, 230f));
            banner.raycastTarget = false;

            Image sword = UiFactory.CreateImage(banner.transform, "Sword", UiArtLibrary.Instance.Sword, new Vector2(170f, 170f));
            UiFactory.Place(sword.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(100f, 0f), new Vector2(170f, 170f));
            sword.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -35f);

            Text title = UiFactory.CreateText(banner.transform, "Title", "THE LAST\nGUARDIAN", 72, UiTheme.Gold, TextAnchor.MiddleLeft);
            UiFactory.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(200f, -12f), new Vector2(440f, 156f));
            title.lineSpacing = 0.9f;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 40;
            title.resizeTextMaxSize = 72;

            Outline outline = title.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.12f, 0.04f, 1f);
            outline.effectDistance = new Vector2(4f, -4f);

            Text subtitle = UiFactory.CreateText(banner.transform, "Subtitle", "Piter: Rescue Aleksandra", 34, UiTheme.TextColor, TextAnchor.MiddleLeft);
            UiFactory.Place(subtitle.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(206f, 16f), new Vector2(434f, 56f));
            subtitle.resizeTextForBestFit = true;
            subtitle.resizeTextMinSize = 22;
            subtitle.resizeTextMaxSize = 34;

            Image footer = UiFactory.CreateRoundedPanel(root, "FooterPanel", new Color(0.1f, 0.06f, 0.03f, 0.7f), false);
            UiFactory.Place(footer.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(70f, 24f), new Vector2(660f, 96f));
            footer.raycastTarget = false;

            footerText = UiFactory.CreateText(footer.transform, "Footer", string.Empty, 26, UiTheme.MutedText, TextAnchor.MiddleCenter);
            UiFactory.Inset(footerText.rectTransform, 12f);
        }

        private void BuildCharacterShowcase(Transform root, UiArtLibrary art)
        {
            RectTransform stage = UiFactory.CreateRect("CharacterShowcase", root);
            UiFactory.Stretch(stage);

            AddCharacter(stage, art, "Malachor", new Vector2(1f, 1f), new Vector2(-280f, -560f), 640f, true, new Color(0.8f, 0.6f, 1f, 0.6f), 0f);
            AddCharacter(stage, art, "Dragon", new Vector2(1f, 1f), new Vector2(-700f, -420f), 470f, true, Color.white, 0.3f);
            AddCharacter(stage, art, "Harpy", new Vector2(1f, 0f), new Vector2(-360f, 420f), 330f, true, Color.white, 0.6f);
            AddCharacter(stage, art, "Golem", new Vector2(1f, 0f), new Vector2(-170f, 0f), 500f, true, Color.white, 0.1f);
            AddCharacter(stage, art, "Skeleton", new Vector2(1f, 0f), new Vector2(-410f, 0f), 430f, true, Color.white, 0.4f);
            AddCharacter(stage, art, "Wolf", new Vector2(1f, 0f), new Vector2(-600f, -10f), 380f, true, Color.white, 0.7f);
            AddCharacter(stage, art, "Goblin", new Vector2(1f, 0f), new Vector2(-770f, 0f), 350f, true, Color.white, 0.2f);
            AddCharacter(stage, art, "Aleksandra", new Vector2(1f, 0f), new Vector2(-1110f, 10f), 430f, false, Color.white, 0.5f);
            AddCharacter(stage, art, "Piter", new Vector2(1f, 0f), new Vector2(-960f, -10f), 560f, false, Color.white, 0f);
        }

        private void BuildMainPanel(Transform root)
        {
            Image card = UiFactory.CreateFramedPanel(root, "MenuCard", new Vector2(660f, 650f));
            UiFactory.Place(card.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70f, -282f), new Vector2(660f, 650f));
            mainPanel = card.rectTransform;

            RectTransform column = UiFactory.CreateRect("Buttons", card.transform);
            UiFactory.Inset(column, 40f);
            UiFactory.AddVerticalLayout(column, 14f, 0, TextAnchor.UpperCenter);

            continueButton = UiFactory.CreateButton(column, "Continue", SceneFlow.ContinueGame, ButtonSize, ButtonColors.Emerald);
            newGameButton = UiFactory.CreateButton(column, "New Game", OnNewGame, ButtonSize, ButtonColors.Gold);
            UiFactory.CreateButton(column, "Select Stage", OpenLevelSelect, ButtonSize, ButtonColors.Sapphire);
            UiFactory.CreateButton(column, "Settings", OpenSettings, ButtonSize, ButtonColors.Amethyst);
            UiFactory.CreateButton(column, "Story & Controls", OpenAbout, ButtonSize, ButtonColors.Teal);

            Button quit = UiFactory.CreateButton(column, "Quit", Quit, ButtonSize, ButtonColors.Crimson);
            quit.gameObject.SetActive(!Application.isMobilePlatform && Application.platform != RuntimePlatform.WebGLPlayer);
        }

        private void ShowMain()
        {
            SaveData save = SaveSystem.Current;
            bool hasProgress = save.HasProgress;

            mainPanel.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(hasProgress);
            UiFactory.SetButtonLabel(continueButton, DescribeContinue(save));

            footerText.text = hasProgress
                ? $"Stages completed: {save.CompletedLevels} / {LevelCatalog.Count}   |   Coins: {save.TotalCoins}\nLast saved: {save.SavedAt}"
                : "A medieval fantasy platformer in ten stages";

            UiFactory.Select(hasProgress ? continueButton : newGameButton);
        }

        private void HideMain()
        {
            mainPanel.gameObject.SetActive(false);
        }

        private void HandleBack()
        {
            if (confirm.IsOpen)
            {
                confirm.Cancel();
            }
            else if (settings.IsOpen)
            {
                settings.Close();
            }
            else if (levelSelect.IsOpen)
            {
                levelSelect.Close();
            }
            else if (about.IsOpen)
            {
                about.Close();
            }
        }

        private void OnNewGame()
        {
            if (!SaveSystem.HasProgress)
            {
                SceneFlow.StartNewGame();

                return;
            }

            HideMain();
            confirm.Show("Start a new game?\nYour current progress will be erased.", SceneFlow.StartNewGame);
        }

        private void OpenLevelSelect()
        {
            HideMain();
            levelSelect.Show();
        }

        private void OpenSettings()
        {
            HideMain();
            settings.Show();
        }

        private void OpenAbout()
        {
            HideMain();
            about.Show();
        }

        private void OnLevelChosen(int levelId)
        {
            SaveData save = SaveSystem.Current;
            bool resume = save.CheckpointLevel == levelId;

            SceneFlow.LoadLevel(levelId, resume);
        }

        private void Quit()
        {
            SettingsStore.Save();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void UpdateBackground()
        {
            float zoom = 1f + BackgroundZoom * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 0.15f));
            backgroundFront.rectTransform.localScale = new Vector3(zoom, zoom, 1f);
            backgroundBack.rectTransform.localScale = new Vector3(zoom, zoom, 1f);

            float fade = Mathf.Clamp01((Time.unscaledTime - crossfadeStart) / CrossfadeSeconds);
            backgroundFront.color = new Color(1f, 1f, 1f, fade);

            if (Time.unscaledTime < nextCycleTime)
            {
                return;
            }

            int unlocked = Mathf.Clamp(SaveSystem.Current.HighestUnlockedLevel, 1, LevelCatalog.Count);
            previewIndex = (previewIndex + 1) % Mathf.Max(3, unlocked);

            backgroundBack.sprite = backgroundFront.sprite;
            backgroundFront.sprite = UiArtLibrary.Instance.GetLevelPreview(previewIndex + 1);
            crossfadeStart = Time.unscaledTime;
            nextCycleTime = Time.unscaledTime + BackgroundCycleSeconds;
        }
    }
}
