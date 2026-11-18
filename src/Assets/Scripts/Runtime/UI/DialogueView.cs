using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Story title cards and typewriter dialogue with speaker portraits.
    /// </summary>
    public sealed class DialogueView : MonoBehaviour
    {
        private const float CharactersPerSecond = 55f;
        private const float InputGuardSeconds = 0.25f;
        private const float PortraitSize = 250f;

        private GameObject titlePanel;
        private GameObject linePanel;
        private Text titleText;
        private Text subtitleText;
        private Text bodyText;
        private Image portrait;
        private Text speakerText;
        private Text lineText;
        private RectTransform lineTextRect;
        private RectTransform speakerRect;
        private DialogueLine[] lines = Array.Empty<DialogueLine>();
        private Action onFinished;
        private int lineIndex;
        private float typedCharacters;
        private float shownAt;
        private bool typing;
        private bool showingTitle;

        public bool IsOpen => gameObject.activeSelf;

        public static DialogueView Create(Transform parent)
        {
            RectTransform root = UiFactory.CreateRect("Dialogue", parent);
            UiFactory.Stretch(root);

            DialogueView view = root.gameObject.AddComponent<DialogueView>();
            view.Build();
            view.gameObject.SetActive(false);

            return view;
        }

        public void ShowTitle(string title, string subtitle, string body, Action finished)
        {
            onFinished = finished;
            showingTitle = true;
            titleText.text = title;
            subtitleText.text = subtitle;
            bodyText.text = body;

            Open();
            titlePanel.SetActive(true);
            linePanel.SetActive(false);
        }

        public void ShowLines(DialogueLine[] dialogueLines, Action finished)
        {
            if (dialogueLines == null || dialogueLines.Length == 0)
            {
                finished?.Invoke();

                return;
            }

            onFinished = finished;
            showingTitle = false;
            lines = dialogueLines;
            lineIndex = 0;

            Open();
            titlePanel.SetActive(false);
            linePanel.SetActive(true);
            ShowLine();
        }

        public void Advance()
        {
            if (Time.unscaledTime - shownAt < InputGuardSeconds)
            {
                return;
            }

            if (showingTitle)
            {
                Finish();

                return;
            }

            if (typing)
            {
                typing = false;
                lineText.text = lines[lineIndex].Text;

                return;
            }

            lineIndex++;

            if (lineIndex >= lines.Length)
            {
                Finish();

                return;
            }

            ShowLine();
        }

        private static bool AdvancePressed()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;

            bool keyboardPressed = keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame);
            bool gamepadPressed = gamepad != null && (gamepad.buttonSouth.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame);

            return keyboardPressed || gamepadPressed;
        }

        private void Build()
        {
            Image overlay = UiFactory.CreatePanel(transform, "ClickCatcher", new Color(0f, 0f, 0f, 0.35f));
            UiFactory.Stretch(overlay.rectTransform);

            Button catcher = overlay.gameObject.AddComponent<Button>();
            catcher.transition = Selectable.Transition.None;
            catcher.onClick.AddListener(Advance);

            BuildTitlePanel();
            BuildLinePanel();
        }

        private void BuildTitlePanel()
        {
            Image panel = UiFactory.CreateFramedPanel(transform, "TitleCard", new Vector2(1300f, 640f));
            panel.raycastTarget = false;
            titlePanel = panel.gameObject;

            titleText = UiFactory.CreateText(panel.transform, "Title", string.Empty, 68, UiTheme.Gold, TextAnchor.MiddleCenter);
            UiFactory.Place(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 100f));

            subtitleText = UiFactory.CreateText(panel.transform, "Subtitle", string.Empty, 38, UiTheme.TextColor, TextAnchor.MiddleCenter);
            UiFactory.Place(subtitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1180f, 110f));

            bodyText = UiFactory.CreateText(panel.transform, "Body", string.Empty, 34, UiTheme.MutedText, TextAnchor.UpperCenter);
            UiFactory.Place(bodyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -280f), new Vector2(1160f, 260f));

            Text hint = UiFactory.CreateText(panel.transform, "Hint", "Press Space / Enter or tap to begin", 28, UiTheme.Gold, TextAnchor.MiddleCenter);
            UiFactory.Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(1100f, 50f));
        }

        private void BuildLinePanel()
        {
            Image panel = UiFactory.CreateFramedPanel(transform, "LinePanel", new Vector2(1640f, 320f));
            panel.raycastTarget = false;
            UiFactory.Place(panel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(1640f, 320f));
            linePanel = panel.gameObject;

            portrait = UiFactory.CreateImage(panel.transform, "Portrait", null, new Vector2(PortraitSize, PortraitSize));
            UiFactory.Place(portrait.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(PortraitSize, PortraitSize));

            speakerText = UiFactory.CreateText(panel.transform, "Speaker", string.Empty, 42, UiTheme.Gold, TextAnchor.MiddleLeft);
            speakerRect = speakerText.rectTransform;

            lineText = UiFactory.CreateText(panel.transform, "Line", string.Empty, 38, UiTheme.TextColor, TextAnchor.UpperLeft);
            lineTextRect = lineText.rectTransform;

            Text hint = UiFactory.CreateText(panel.transform, "Hint", "Space / Enter / tap  >", 24, UiTheme.MutedText, TextAnchor.MiddleRight);
            UiFactory.Place(hint.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 16f), new Vector2(600f, 40f));
        }

        private void Open()
        {
            shownAt = Time.unscaledTime;
            gameObject.SetActive(true);
        }

        private void ShowLine()
        {
            DialogueLine line = lines[lineIndex];
            Sprite face = UiArtLibrary.Instance.GetPortrait(line.Speaker);
            bool hasPortrait = face != null;
            float textLeft = hasPortrait ? PortraitSize + 70f : 40f;

            portrait.gameObject.SetActive(hasPortrait);
            portrait.sprite = face;
            portrait.color = Color.white;

            UiFactory.Place(speakerRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(textLeft, -24f), new Vector2(1600f - textLeft, 56f));
            UiFactory.Place(lineTextRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(textLeft, -92f), new Vector2(1600f - textLeft - 30f, 200f));

            speakerText.text = line.Speaker;
            lineText.text = string.Empty;
            typedCharacters = 0f;
            typing = true;
            shownAt = Time.unscaledTime;
        }

        private void Update()
        {
            if (!showingTitle && typing && lineIndex < lines.Length)
            {
                string full = lines[lineIndex].Text ?? string.Empty;
                typedCharacters += Time.unscaledDeltaTime * CharactersPerSecond;

                int count = Mathf.Min(full.Length, Mathf.FloorToInt(typedCharacters));
                lineText.text = full.Substring(0, count);

                if (count >= full.Length)
                {
                    typing = false;
                }
            }

            if (AdvancePressed())
            {
                Advance();
            }
        }

        private void Finish()
        {
            gameObject.SetActive(false);

            Action callback = onFinished;
            onFinished = null;

            callback?.Invoke();
        }
    }
}
