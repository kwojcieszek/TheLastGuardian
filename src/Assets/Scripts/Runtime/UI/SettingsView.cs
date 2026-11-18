using System;
using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Sound, display, touch and dialogue preferences.
    /// </summary>
    public sealed class SettingsView : MonoBehaviour
    {
        private const float VolumeStep = 0.1f;

        private static readonly Vector2 ButtonSize = new Vector2(640f, 90f);
        private static readonly Vector2 SmallButtonSize = new Vector2(110f, 90f);

        private Text volumeText;
        private Button volumeDownButton;
        private Button fullscreenButton;
        private Button touchButton;
        private Button dialogueButton;

        public bool IsOpen => gameObject.activeSelf;

        public event Action Closed;

        public static SettingsView Create(Transform parent)
        {
            RectTransform root = MenuPanel.CreateRoot(parent, "Settings", true);

            SettingsView view = root.gameObject.AddComponent<SettingsView>();
            view.Build();
            view.gameObject.SetActive(false);

            return view;
        }

        public void Show()
        {
            Refresh();
            gameObject.SetActive(true);

            UiFactory.Select(volumeDownButton);
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            SettingsStore.Save();
            gameObject.SetActive(false);

            Closed?.Invoke();
        }

        private static string DescribeTouchMode(TouchControlsMode mode)
        {
            switch (mode)
            {
                case TouchControlsMode.AlwaysOn:
                    return "Always on";
                case TouchControlsMode.Off:
                    return "Off";
                default:
                    return "Auto";
            }
        }

        private void Build()
        {
            RectTransform window = MenuPanel.CreateWindow(transform, "Settings", new Vector2(820f, 820f));
            RectTransform column = MenuPanel.CreateButtonColumn(window, 150f, 22f);

            RectTransform volumeRow = UiFactory.CreateRect("VolumeRow", column);
            volumeRow.sizeDelta = new Vector2(ButtonSize.x, ButtonSize.y);
            UiFactory.AddHorizontalLayout(volumeRow, 20f, TextAnchor.MiddleCenter);

            volumeDownButton = UiFactory.CreateButton(volumeRow, "-", () => ChangeVolume(-VolumeStep), SmallButtonSize, ButtonColors.Sapphire, 54);

            Image volumeBox = UiFactory.CreateRoundedPanel(volumeRow, "VolumeLabel", new Color(0.1f, 0.06f, 0.03f, 0.55f), false);
            volumeBox.rectTransform.sizeDelta = new Vector2(380f, ButtonSize.y);

            volumeText = UiFactory.CreateText(volumeBox.transform, "Text", string.Empty, 38, UiTheme.TextColor, TextAnchor.MiddleCenter);
            UiFactory.Stretch(volumeText.rectTransform);

            UiFactory.CreateButton(volumeRow, "+", () => ChangeVolume(VolumeStep), SmallButtonSize, ButtonColors.Sapphire, 54);

            fullscreenButton = UiFactory.CreateButton(column, string.Empty, ToggleFullscreen, ButtonSize, ButtonColors.Teal);
            fullscreenButton.gameObject.SetActive(SettingsStore.SupportsFullscreenToggle);

            touchButton = UiFactory.CreateButton(column, string.Empty, CycleTouchControls, ButtonSize, ButtonColors.Amethyst);
            dialogueButton = UiFactory.CreateButton(column, string.Empty, ToggleDialogues, ButtonSize, ButtonColors.Gold);

            UiFactory.CreateButton(column, "Back", Close, ButtonSize, ButtonColors.Crimson);
        }

        private void ChangeVolume(float delta)
        {
            GameSettings settings = SettingsStore.Current;
            settings.SoundVolume = Mathf.Round((settings.SoundVolume + delta) * 10f) / 10f;

            SettingsStore.Apply();
            Refresh();
        }

        private void ToggleFullscreen()
        {
            GameSettings settings = SettingsStore.Current;
            settings.Fullscreen = !settings.Fullscreen;

            SettingsStore.Apply();
            Refresh();
        }

        private void CycleTouchControls()
        {
            GameSettings settings = SettingsStore.Current;
            settings.TouchControls = (TouchControlsMode)(((int)settings.TouchControls + 1) % 3);

            Refresh();
        }

        private void ToggleDialogues()
        {
            GameSettings settings = SettingsStore.Current;
            settings.SkipSeenDialogues = !settings.SkipSeenDialogues;

            Refresh();
        }

        private void Refresh()
        {
            GameSettings settings = SettingsStore.Current;

            volumeText.text = $"Sound: {Mathf.RoundToInt(settings.SoundVolume * 100f)}%";
            UiFactory.SetButtonLabel(fullscreenButton, "Fullscreen: " + (settings.Fullscreen ? "On" : "Off"));
            UiFactory.SetButtonLabel(touchButton, "Touch controls: " + DescribeTouchMode(settings.TouchControls));
            UiFactory.SetButtonLabel(dialogueButton, "Story dialogues: " + (settings.SkipSeenDialogues ? "Skip" : "Show"));
        }
    }
}
