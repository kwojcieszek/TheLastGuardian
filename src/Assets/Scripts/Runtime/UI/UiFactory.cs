using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Builds uGUI elements from code so that screens do not depend on hand-made prefabs.
    /// </summary>
    public static class UiFactory
    {
        private const string BuiltinFontName = "LegacyRuntime.ttf";
        private const int UiLayer = 5;
        private const float FrameThickness = 5f;
        private const float ButtonFrameThickness = 4f;
        private const float EdgeThickness = 2f;

        private static Font font;

        public static Vector2 ReferenceResolution { get; } = new Vector2(1920f, 1080f);

        public static Font Font
        {
            get
            {
                if (font == null)
                {
                    font = Resources.GetBuiltinResource<Font>(BuiltinFontName);
                }

                return font;
            }
        }

        public static Canvas CreateCanvas(string name, int sortingOrder)
        {
            GameObject host = new GameObject(name, typeof(RectTransform));
            host.layer = UiLayer;

            Canvas canvas = host.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = host.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            host.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject host = new GameObject(name, typeof(RectTransform));
            host.layer = UiLayer;

            RectTransform rect = host.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            return rect;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static Image CreatePanel(Transform parent, string name, Color color)
        {
            RectTransform rect = CreateRect(name, parent);

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;

            return image;
        }

        public static Image CreateRoundedPanel(Transform parent, string name, Color color, bool shaded)
        {
            Image image = CreatePanel(parent, name, color);
            image.sprite = shaded ? RoundedSprites.Gradient : RoundedSprites.Flat;
            image.type = Image.Type.Sliced;

            return image;
        }

        public static Image CreateFramedPanel(Transform parent, string name, Vector2 size)
        {
            Image frame = CreateRoundedPanel(parent, name, UiTheme.FrameColor, true);
            Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);

            Shadow shadow = frame.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            shadow.effectDistance = new Vector2(0f, -10f);

            Image edge = CreateRoundedPanel(frame.transform, "Edge", UiTheme.FrameEdge, false);
            Inset(edge.rectTransform, FrameThickness);
            edge.raycastTarget = false;

            Image inner = CreateRoundedPanel(edge.transform, "Inner", UiTheme.PanelColor, true);
            Inset(inner.rectTransform, EdgeThickness);
            inner.raycastTarget = false;

            return frame;
        }

        public static void Inset(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        public static Image CreateOverlay(Transform parent, string name)
        {
            Image overlay = CreatePanel(parent, name, UiTheme.OverlayColor);
            Stretch(overlay.rectTransform);

            return overlay;
        }

        public static Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor alignment)
        {
            RectTransform rect = CreateRect(name, parent);

            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Font;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            Shadow shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return text;
        }

        public static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.sizeDelta = size;

            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            if (sprite == null)
            {
                image.color = Color.clear;
            }

            return image;
        }

        public static Button CreateButton(Transform parent, string label, UnityAction onClick, Vector2 size, int fontSize = 38)
        {
            return CreateButton(parent, label, onClick, size, ButtonColors.Bronze, fontSize);
        }

        public static Button CreateButton(Transform parent, string label, UnityAction onClick, Vector2 size, Color color, int fontSize = 38)
        {
            Image frame = CreateRoundedPanel(parent, label + " Button", UiTheme.FrameColor, true);
            frame.rectTransform.sizeDelta = size;

            LayoutElement layout = frame.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;

            Shadow shadow = frame.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(0f, -6f);

            Image edge = CreateRoundedPanel(frame.transform, "Edge", UiTheme.FrameEdge, false);
            Inset(edge.rectTransform, ButtonFrameThickness);
            edge.raycastTarget = false;

            Image face = CreateRoundedPanel(edge.transform, "Face", Color.white, true);
            Inset(face.rectTransform, EdgeThickness);

            Image gloss = CreateRoundedPanel(face.transform, "Gloss", new Color(1f, 0.95f, 0.8f, 0.08f), false);
            gloss.raycastTarget = false;
            gloss.rectTransform.anchorMin = new Vector2(0f, 0.52f);
            gloss.rectTransform.anchorMax = new Vector2(1f, 1f);
            gloss.rectTransform.offsetMin = new Vector2(8f, 0f);
            gloss.rectTransform.offsetMax = new Vector2(-8f, -5f);

            Button button = frame.gameObject.AddComponent<Button>();
            button.targetGraphic = face;

            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = ButtonColors.Lighten(color, 0.18f);
            colors.selectedColor = ButtonColors.Lighten(color, 0.18f);
            colors.pressedColor = ButtonColors.Darken(color, 0.25f);
            colors.disabledColor = UiTheme.ButtonDisabled;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            frame.gameObject.AddComponent<ButtonHoverEffect>();
            button.onClick.AddListener(() => GameAudio.Play(SoundEffect.Click));

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            Text text = CreateText(frame.transform, "Label", label, fontSize, UiTheme.TextColor, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);

            Shadow outline = text.gameObject.AddComponent<Shadow>();
            outline.effectColor = new Color(0.08f, 0.04f, 0.02f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            return button;
        }

        public static Button CreateIconButton(Transform parent, string name, Sprite icon, UnityAction onClick, Vector2 size)
        {
            Image image = CreateImage(parent, name, icon, size);
            image.raycastTarget = true;

            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            image.gameObject.AddComponent<ButtonHoverEffect>();

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            return button;
        }

        public static void SetButtonLabel(Button button, string label)
        {
            Text text = button.GetComponentInChildren<Text>();

            if (text != null)
            {
                text.text = label;
            }
        }

        public static VerticalLayoutGroup AddVerticalLayout(Component target, float spacing, int padding, TextAnchor alignment)
        {
            VerticalLayoutGroup layout = target.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.childAlignment = alignment;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            return layout;
        }

        public static HorizontalLayoutGroup AddHorizontalLayout(Component target, float spacing, TextAnchor alignment)
        {
            HorizontalLayoutGroup layout = target.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            return layout;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject host = new GameObject("EventSystem");
            host.AddComponent<EventSystem>();

            InputSystemUIInputModule module = host.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        public static void Select(Selectable selectable)
        {
            EventSystem eventSystem = EventSystem.current;

            if (selectable == null || eventSystem == null)
            {
                return;
            }

            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(selectable.gameObject);
        }

        public static string FormatTime(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));

            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
