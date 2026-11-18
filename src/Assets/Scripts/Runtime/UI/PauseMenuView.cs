using System;
using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Pause screen shown during a level.
    /// </summary>
    public sealed class PauseMenuView : MonoBehaviour
    {
        private static readonly Vector2 ButtonSize = new Vector2(560f, 96f);

        private Button resumeButton;
        private Text infoText;

        public bool IsOpen => gameObject.activeSelf;

        public event Action ResumeRequested;

        public event Action RestartCheckpointRequested;

        public event Action RestartLevelRequested;

        public event Action SettingsRequested;

        public event Action MainMenuRequested;

        public static PauseMenuView Create(Transform parent)
        {
            RectTransform root = MenuPanel.CreateRoot(parent, "PauseMenu", true);

            PauseMenuView view = root.gameObject.AddComponent<PauseMenuView>();
            view.Build();
            view.gameObject.SetActive(false);

            return view;
        }

        public void Show(string info)
        {
            infoText.text = info;
            gameObject.SetActive(true);

            UiFactory.Select(resumeButton);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Build()
        {
            RectTransform window = MenuPanel.CreateWindow(transform, "Paused", new Vector2(760f, 860f));

            infoText = UiFactory.CreateText(window, "Info", string.Empty, 30, UiTheme.MutedText, TextAnchor.MiddleCenter);
            UiFactory.Place(infoText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(700f, 80f));

            RectTransform column = MenuPanel.CreateButtonColumn(window, 220f, 18f);

            resumeButton = UiFactory.CreateButton(column, "Resume", () => ResumeRequested?.Invoke(), ButtonSize, ButtonColors.Emerald);
            UiFactory.CreateButton(column, "Restart from checkpoint", () => RestartCheckpointRequested?.Invoke(), ButtonSize, ButtonColors.Sapphire);
            UiFactory.CreateButton(column, "Restart stage", () => RestartLevelRequested?.Invoke(), ButtonSize, ButtonColors.Gold);
            UiFactory.CreateButton(column, "Settings", () => SettingsRequested?.Invoke(), ButtonSize, ButtonColors.Amethyst);
            UiFactory.CreateButton(column, "Save & main menu", () => MainMenuRequested?.Invoke(), ButtonSize, ButtonColors.Crimson);
        }
    }
}
