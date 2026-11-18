using System;
using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Yes / no confirmation dialog.
    /// </summary>
    public sealed class ConfirmView : MonoBehaviour
    {
        private static readonly Vector2 ButtonSize = new Vector2(300f, 92f);

        private Text messageText;
        private Button cancelButton;
        private Action confirmed;

        public bool IsOpen => gameObject.activeSelf;

        public event Action Closed;

        public static ConfirmView Create(Transform parent)
        {
            RectTransform root = MenuPanel.CreateRoot(parent, "Confirm", true);

            ConfirmView view = root.gameObject.AddComponent<ConfirmView>();
            view.Build();
            view.gameObject.SetActive(false);

            return view;
        }

        public void Show(string message, Action onConfirmed)
        {
            messageText.text = message;
            confirmed = onConfirmed;
            gameObject.SetActive(true);

            UiFactory.Select(cancelButton);
        }

        public void Cancel()
        {
            confirmed = null;
            Close();
        }

        private void Build()
        {
            RectTransform window = MenuPanel.CreateWindow(transform, "Are you sure?", new Vector2(860f, 480f));

            messageText = UiFactory.CreateText(window, "Message", string.Empty, 36, UiTheme.TextColor, TextAnchor.MiddleCenter);
            UiFactory.Place(messageText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(780f, 150f));

            RectTransform row = UiFactory.CreateRect("Buttons", window);
            UiFactory.Place(row, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(700f, 100f));
            UiFactory.AddHorizontalLayout(row, 40f, TextAnchor.MiddleCenter);

            UiFactory.CreateButton(row, "Yes", Confirm, ButtonSize, ButtonColors.Crimson);
            cancelButton = UiFactory.CreateButton(row, "No", Cancel, ButtonSize, ButtonColors.Emerald);
        }

        private void Confirm()
        {
            Action action = confirmed;
            confirmed = null;
            Close();

            action?.Invoke();
        }

        private void Close()
        {
            gameObject.SetActive(false);

            Closed?.Invoke();
        }
    }
}
