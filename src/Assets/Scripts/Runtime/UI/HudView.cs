using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// In-game heads-up display: hearts, coins, potions, progress, objective, boss bar and messages.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        private const int MaxHearts = 6;
        private const float ToastSeconds = 2.6f;
        private const float ToastFadeSeconds = 0.4f;

        private readonly List<Image> hearts = new List<Image>();
        private LevelSession session;
        private Text coinsText;
        private Text potionsText;
        private Text levelText;
        private Text objectiveText;
        private Text toastText;
        private Text bossNameText;
        private RectTransform progressFill;
        private RectTransform bossFill;
        private GameObject bossBar;
        private float toastUntil;
        private int shownCoins = -1;
        private int shownPotions = -1;
        private int shownHealth = -1;
        private int shownMaxHealth = -1;

        public event Action PauseRequested;

        public static HudView Create(Transform parent, LevelSession session)
        {
            RectTransform root = UiFactory.CreateRect("HUD", parent);
            UiFactory.Stretch(root);

            HudView view = root.gameObject.AddComponent<HudView>();
            view.session = session;
            view.Build();

            return view;
        }

        public void ShowToast(string message)
        {
            toastText.text = message;
            toastUntil = Time.unscaledTime + ToastSeconds;
        }

        private void Build()
        {
            UiArtLibrary art = UiArtLibrary.Instance;

            BuildStatusPanel(art);
            BuildProgress();
            BuildBossBar();

            Button pause = UiFactory.CreateIconButton(transform, "PauseButton", art.Pause, () => PauseRequested?.Invoke(), new Vector2(110f, 110f));
            UiFactory.Place((RectTransform)pause.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(110f, 110f));

            toastText = UiFactory.CreateText(transform, "Toast", string.Empty, 44, UiTheme.TextColor, TextAnchor.MiddleCenter);
            UiFactory.Place(toastText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(1500f, 140f));

            Outline toastOutline = toastText.gameObject.AddComponent<Outline>();
            toastOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            toastOutline.effectDistance = new Vector2(2f, -2f);
        }

        private void BuildStatusPanel(UiArtLibrary art)
        {
            Image backdrop = UiFactory.CreateRoundedPanel(transform, "StatusBackdrop", new Color(0.1f, 0.06f, 0.03f, 0.5f), false);
            UiFactory.Place(backdrop.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(480f, 176f));
            backdrop.raycastTarget = false;

            RectTransform heartsRow = UiFactory.CreateRect("Hearts", transform);
            UiFactory.Place(heartsRow, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -24f), new Vector2(480f, 80f));
            UiFactory.AddHorizontalLayout(heartsRow, 6f, TextAnchor.MiddleLeft);

            for (int index = 0; index < MaxHearts; index++)
            {
                Image heart = UiFactory.CreateImage(heartsRow, "Heart_" + index, art.Heart, new Vector2(72f, 72f));
                hearts.Add(heart);
            }

            Image coin = UiFactory.CreateImage(transform, "CoinIcon", art.Coin, new Vector2(60f, 60f));
            UiFactory.Place(coin.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -112f), new Vector2(60f, 60f));

            coinsText = UiFactory.CreateText(transform, "Coins", "0", 40, UiTheme.Gold, TextAnchor.MiddleLeft);
            UiFactory.Place(coinsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(104f, -112f), new Vector2(200f, 60f));

            Image potion = UiFactory.CreateImage(transform, "PotionIcon", art.Potion, new Vector2(60f, 60f));
            UiFactory.Place(potion.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(230f, -112f), new Vector2(60f, 60f));

            potionsText = UiFactory.CreateText(transform, "Potions", "0", 40, UiTheme.TextColor, TextAnchor.MiddleLeft);
            UiFactory.Place(potionsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(296f, -112f), new Vector2(200f, 60f));
        }

        private void BuildProgress()
        {
            levelText = UiFactory.CreateText(transform, "LevelName", string.Empty, 34, UiTheme.TextColor, TextAnchor.MiddleCenter);
            UiFactory.Place(levelText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(900f, 50f));

            Image background = UiFactory.CreateRoundedPanel(transform, "ProgressBar", UiTheme.BarBackground, false);
            UiFactory.Place(background.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(560f, 22f));

            Image fill = UiFactory.CreateRoundedPanel(background.transform, "Fill", UiTheme.ProgressFill, true);
            progressFill = fill.rectTransform;
            UiFactory.Stretch(progressFill);

            objectiveText = UiFactory.CreateText(transform, "Objective", string.Empty, 30, UiTheme.Gold, TextAnchor.UpperCenter);
            UiFactory.Place(objectiveText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(1000f, 90f));
        }

        private void BuildBossBar()
        {
            RectTransform root = UiFactory.CreateRect("BossBar", transform);
            UiFactory.Place(root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(820f, 90f));
            bossBar = root.gameObject;

            bossNameText = UiFactory.CreateText(root, "BossName", string.Empty, 34, UiTheme.TextColor, TextAnchor.MiddleCenter);
            UiFactory.Place(bossNameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(820f, 44f));

            Image background = UiFactory.CreateRoundedPanel(root, "Bar", UiTheme.BarBackground, false);
            UiFactory.Place(background.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(820f, 30f));

            Outline outline = background.gameObject.AddComponent<Outline>();
            outline.effectColor = UiTheme.Gold;
            outline.effectDistance = new Vector2(2f, -2f);

            Image fill = UiFactory.CreateRoundedPanel(background.transform, "Fill", UiTheme.BossFill, true);
            bossFill = fill.rectTransform;
            UiFactory.Stretch(bossFill);

            bossBar.SetActive(false);
        }

        private void Update()
        {
            if (session == null || session.Player == null)
            {
                return;
            }

            RefreshHearts();
            RefreshCounters();
            RefreshProgress();
            RefreshBoss();
            RefreshToast();
        }

        private void RefreshHearts()
        {
            Health health = session.Player.Health;

            if (health.CurrentHealth == shownHealth && health.MaxHealth == shownMaxHealth)
            {
                return;
            }

            shownHealth = health.CurrentHealth;
            shownMaxHealth = health.MaxHealth;

            for (int index = 0; index < hearts.Count; index++)
            {
                hearts[index].gameObject.SetActive(index < shownMaxHealth);
                hearts[index].color = index < shownHealth ? Color.white : UiTheme.LostHeart;
            }
        }

        private void RefreshCounters()
        {
            if (session.Coins != shownCoins)
            {
                shownCoins = session.Coins;
                coinsText.text = shownCoins.ToString();
            }

            if (session.Potions != shownPotions)
            {
                shownPotions = session.Potions;
                potionsText.text = $"{shownPotions}/{SaveData.MaxPotions}";
            }
        }

        private void RefreshProgress()
        {
            levelText.text = session.LevelTitle;
            progressFill.anchorMax = new Vector2(Mathf.Max(0.001f, session.Progress), 1f);
            objectiveText.text = session.ObjectiveStatus;
        }

        private void RefreshBoss()
        {
            EnemyController boss = session.Guardian;
            bool visible = session.BossEngaged && boss != null && !boss.IsDead;

            if (bossBar.activeSelf != visible)
            {
                bossBar.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            bossNameText.text = boss.DisplayName;
            bossFill.anchorMax = new Vector2(Mathf.Max(0.001f, boss.Health.Fraction), 1f);
        }

        private void RefreshToast()
        {
            float remaining = toastUntil - Time.unscaledTime;
            Color color = toastText.color;
            color.a = Mathf.Clamp01(remaining / ToastFadeSeconds);
            toastText.color = color;
        }
    }
}
