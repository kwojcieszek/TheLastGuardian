using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Shared construction helpers for modal menu screens.
    /// </summary>
    public static class MenuPanel
    {
        public static RectTransform CreateRoot(Transform parent, string name, bool withOverlay)
        {
            RectTransform root = UiFactory.CreateRect(name, parent);
            UiFactory.Stretch(root);

            if (withOverlay)
            {
                Image overlay = UiFactory.CreateOverlay(root, "Overlay");
                overlay.raycastTarget = true;
            }

            return root;
        }

        public static RectTransform CreateWindow(Transform parent, string title, Vector2 size)
        {
            Image panel = UiFactory.CreateFramedPanel(parent, "Window", size);

            Text heading = UiFactory.CreateText(panel.transform, "Title", title, 64, UiTheme.Gold, TextAnchor.MiddleCenter);
            UiFactory.Place(heading.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(size.x - 60f, 90f));

            return panel.rectTransform;
        }

        public static RectTransform CreateButtonColumn(Transform parent, float top, float spacing)
        {
            RectTransform column = UiFactory.CreateRect("Buttons", parent);
            column.anchorMin = new Vector2(0f, 0f);
            column.anchorMax = new Vector2(1f, 1f);
            column.offsetMin = new Vector2(0f, 30f);
            column.offsetMax = new Vector2(0f, -top);

            UiFactory.AddVerticalLayout(column, spacing, 0, TextAnchor.UpperCenter);

            return column;
        }
    }
}
