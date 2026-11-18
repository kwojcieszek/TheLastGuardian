using System;
using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Story premise and controls reference.
    /// </summary>
    public sealed class AboutView : MonoBehaviour
    {
        private const string Story =
            "In a medieval fantasy kingdom, the evil sorcerer Malachor abducts Aleksandra to steal her rare magic, the Light of Dawn. " +
            "Her younger brother Piter, a novice warrior, follows the violet trace of the abduction portal through ten dangerous lands " +
            "to save her.";

        private const string Controls =
            "Move: A / D or Arrow keys      Jump: Space / W  (press again in the air for a double jump)\n" +
            "Attack: J / K / Ctrl      Drink potion: E / Q      Pause: Esc / P\n" +
            "Gamepad: Stick / D-pad, A jump, X attack, Y potion, Start pause\n" +
            "Touch: on-screen buttons (enable them in Settings)\n\n" +
            "Jump on enemies to stomp them. Reflect Malachor's orbs with your sword.\n" +
            "Checkpoints heal you and save the game automatically.";

        private Button backButton;

        public bool IsOpen => gameObject.activeSelf;

        public event Action Closed;

        public static AboutView Create(Transform parent)
        {
            RectTransform root = MenuPanel.CreateRoot(parent, "About", true);

            AboutView view = root.gameObject.AddComponent<AboutView>();
            view.Build();
            view.gameObject.SetActive(false);

            return view;
        }

        public void Show()
        {
            gameObject.SetActive(true);

            UiFactory.Select(backButton);
        }

        public void Close()
        {
            gameObject.SetActive(false);

            Closed?.Invoke();
        }

        private void Build()
        {
            RectTransform window = MenuPanel.CreateWindow(transform, "Story & Controls", new Vector2(1500f, 900f));

            Text story = UiFactory.CreateText(window, "Story", Story, 34, UiTheme.TextColor, TextAnchor.UpperCenter);
            UiFactory.Place(story.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(1380f, 180f));

            Text controls = UiFactory.CreateText(window, "Controls", Controls, 30, UiTheme.MutedText, TextAnchor.UpperCenter);
            controls.lineSpacing = 1.15f;
            UiFactory.Place(controls.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(1380f, 400f));

            backButton = UiFactory.CreateButton(window, "Back", Close, new Vector2(420f, 90f), ButtonColors.Crimson);
            UiFactory.Place((RectTransform)backButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(420f, 90f));
        }
    }
}
