using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// On-screen buttons for phones and tablets.
    /// </summary>
    public sealed class TouchControlsView : MonoBehaviour
    {
        private const float DirectionSize = 190f;
        private const float ActionSize = 210f;
        private const float PotionSize = 140f;

        private LevelSession session;
        private CanvasGroup group;

        public static TouchControlsView Create(Transform parent, LevelSession session)
        {
            RectTransform root = UiFactory.CreateRect("TouchControls", parent);
            UiFactory.Stretch(root);

            TouchControlsView view = root.gameObject.AddComponent<TouchControlsView>();
            view.session = session;
            view.Build();

            return view;
        }

        private void Build()
        {
            UiArtLibrary art = UiArtLibrary.Instance;
            group = gameObject.AddComponent<CanvasGroup>();

            CreateButton("Left", art.Jump, TouchButtonAction.Left, new Vector2(0f, 0f), new Vector2(40f, 40f), DirectionSize, 90f);
            CreateButton("Right", art.Jump, TouchButtonAction.Right, new Vector2(0f, 0f), new Vector2(60f + DirectionSize, 40f), DirectionSize, -90f);
            CreateButton("Jump", art.Jump, TouchButtonAction.Jump, new Vector2(1f, 0f), new Vector2(-40f - ActionSize, 40f), ActionSize, 0f);
            CreateButton("Attack", art.Attack, TouchButtonAction.Attack, new Vector2(1f, 0f), new Vector2(-70f - ActionSize * 2f, 40f), ActionSize, 0f);
            CreateButton("Potion", art.Potion, TouchButtonAction.Potion, new Vector2(1f, 0f), new Vector2(-40f - PotionSize, 60f + ActionSize), PotionSize, 0f);

            Refresh();
        }

        private void CreateButton(string name, Sprite sprite, TouchButtonAction action, Vector2 anchor, Vector2 position, float size, float rotation)
        {
            Image image = UiFactory.CreateImage(transform, name, sprite, new Vector2(size, size));
            image.raycastTarget = true;
            UiFactory.Place(image.rectTransform, anchor, Vector2.zero, position, new Vector2(size, size));

            if (!Mathf.Approximately(rotation, 0f))
            {
                image.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                image.rectTransform.anchoredPosition = position + new Vector2(size * 0.5f, size * 0.5f);
                image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            }

            TouchButton button = image.gameObject.AddComponent<TouchButton>();
            button.Action = action;
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            bool visible = SettingsStore.TouchControlsVisible && session != null && session.State == LevelState.Playing;

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;

            if (!visible)
            {
                TouchInputState.Reset();
            }
        }
    }
}
