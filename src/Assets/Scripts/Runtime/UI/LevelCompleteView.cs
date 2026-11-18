using System;
using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Stage results with next / replay / menu options.
    /// </summary>
    public sealed class LevelCompleteView : MonoBehaviour
    {
        private static readonly Vector2 ButtonSize = new Vector2(560f, 92f);

        private Text levelText;
        private Text statsText;
        private Button nextButton;
        private Button replayButton;

        public event Action NextRequested;

        public event Action ReplayRequested;

        public event Action MainMenuRequested;

        public static LevelCompleteView Create(Transform parent)
        {
            RectTransform root = MenuPanel.CreateRoot(parent, "LevelComplete", true);

            LevelCompleteView view = root.gameObject.AddComponent<LevelCompleteView>();
            view.Build();
            view.gameObject.SetActive(false);

            return view;
        }

        public void Show(LevelCompletionSummary summary)
        {
            levelText.text = summary.LevelTitle;
            statsText.text =
                $"Coins: {summary.Coins} / {summary.TotalCoins}\n" +
                $"Secret chests: {summary.Chests} / {summary.TotalChests}\n" +
                $"Enemies defeated: {summary.EnemiesDefeated}\n" +
                $"Time: {UiFactory.FormatTime(summary.TimeSeconds)}\n" +
                "Progress saved.";

            nextButton.gameObject.SetActive(summary.HasNextLevel);
            gameObject.SetActive(true);

            UiFactory.Select(summary.HasNextLevel ? nextButton : replayButton);
        }

        private void Build()
        {
            RectTransform window = MenuPanel.CreateWindow(transform, "Stage Complete!", new Vector2(900f, 900f));

            levelText = UiFactory.CreateText(window, "Level", string.Empty, 40, UiTheme.TextColor, TextAnchor.MiddleCenter);
            UiFactory.Place(levelText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -125f), new Vector2(840f, 60f));

            statsText = UiFactory.CreateText(window, "Stats", string.Empty, 36, UiTheme.MutedText, TextAnchor.UpperCenter);
            statsText.lineSpacing = 1.15f;
            UiFactory.Place(statsText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(840f, 280f));

            RectTransform column = MenuPanel.CreateButtonColumn(window, 530f, 16f);

            nextButton = UiFactory.CreateButton(column, "Next stage", () => NextRequested?.Invoke(), ButtonSize, ButtonColors.Emerald);
            replayButton = UiFactory.CreateButton(column, "Replay stage", () => ReplayRequested?.Invoke(), ButtonSize, ButtonColors.Sapphire);
            UiFactory.CreateButton(column, "Main menu", () => MainMenuRequested?.Invoke(), ButtonSize, ButtonColors.Crimson);
        }
    }
}
