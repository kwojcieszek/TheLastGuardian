using System;
using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Final screen with the story epilogue and scrolling credits.
    /// </summary>
    public sealed class EndingView : MonoBehaviour
    {
        private const float CreditsSpeed = 45f;
        private const float CreditsStartY = 180f;
        private const float CreditsEndY = 1200f;

        private const string Epilogue =
            "The magical barrier shatters and Malachor's spell fades forever.\n" +
            "Aleksandra's Light of Dawn shines again, and the siblings return home together.";

        private const string Credits =
            "THE LAST GUARDIAN\nPiter: Rescue Aleksandra\n\n" +
            "A medieval fantasy adventure in ten stages\n\n" +
            "Village of Oaks  -  Whispering Forest  -  Ruins of the Watchtower\n" +
            "Shadow Mine  -  Waterfall Valley  -  Castle on the Hill\n" +
            "Dragon Pass  -  Dark Catacombs  -  Sorcerer's Tower\n" +
            "Hall of the Final Spell\n\n" +
            "Starring\nPiter, the brave younger brother\nAleksandra, keeper of the Light of Dawn\nMalachor, the dark sorcerer\n\n" +
            "Thank you for playing!";

        private Text statsText;
        private RectTransform creditsRect;
        private Button menuButton;

        public event Action MainMenuRequested;

        public static EndingView Create(Transform parent)
        {
            RectTransform root = MenuPanel.CreateRoot(parent, "Ending", false);

            EndingView view = root.gameObject.AddComponent<EndingView>();
            view.Build();
            view.gameObject.SetActive(false);

            return view;
        }

        public void Show(int totalCoins, int completedLevels)
        {
            statsText.text = $"Stages completed: {completedLevels} / {LevelCatalog.Count}     Coins collected: {totalCoins}";
            creditsRect.anchoredPosition = new Vector2(0f, CreditsStartY);
            gameObject.SetActive(true);

            UiFactory.Select(menuButton);
        }

        private void Build()
        {
            UiArtLibrary art = UiArtLibrary.Instance;

            Image background = UiFactory.CreatePanel(transform, "Background", new Color(0.04f, 0.03f, 0.06f, 1f));
            UiFactory.Stretch(background.rectTransform);
            background.raycastTarget = true;

            Sprite preview = art.GetLevelPreview(1);

            if (preview != null)
            {
                Image village = UiFactory.CreateImage(transform, "Village", preview, Vector2.zero);
                UiFactory.Stretch(village.rectTransform);
                village.preserveAspect = false;
                village.color = new Color(1f, 1f, 1f, 0.35f);
            }

            Text title = UiFactory.CreateText(transform, "Title", "Aleksandra is free!", 96, UiTheme.Gold, TextAnchor.MiddleCenter);
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1600f, 130f));

            Text epilogue = UiFactory.CreateText(transform, "Epilogue", Epilogue, 36, UiTheme.TextColor, TextAnchor.MiddleCenter);
            UiFactory.Place(epilogue.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(1500f, 120f));

            Image piter = UiFactory.CreateImage(transform, "Piter", art.Piter, new Vector2(440f, 440f));
            UiFactory.Place(piter.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(60f, 20f), new Vector2(440f, 440f));

            Image sister = UiFactory.CreateImage(transform, "Aleksandra", art.Aleksandra, new Vector2(440f, 440f));
            UiFactory.Place(sister.rectTransform, new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-280f, 20f), new Vector2(440f, 440f));
            sister.rectTransform.localScale = new Vector3(-1f, 1f, 1f);

            RectTransform creditsMask = UiFactory.CreateRect("CreditsArea", transform);
            UiFactory.Place(creditsMask, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(900f, 440f));
            creditsMask.gameObject.AddComponent<RectMask2D>();

            Text credits = UiFactory.CreateText(creditsMask, "Credits", Credits, 32, UiTheme.MutedText, TextAnchor.UpperCenter);
            creditsRect = credits.rectTransform;
            UiFactory.Place(creditsRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, CreditsStartY), new Vector2(900f, 900f));

            statsText = UiFactory.CreateText(transform, "Stats", string.Empty, 32, UiTheme.Gold, TextAnchor.MiddleCenter);
            UiFactory.Place(statsText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(1200f, 50f));

            menuButton = UiFactory.CreateButton(transform, "Return to main menu", () => MainMenuRequested?.Invoke(), new Vector2(600f, 96f), ButtonColors.Gold);
            UiFactory.Place((RectTransform)menuButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(600f, 96f));
        }

        private void Update()
        {
            Vector2 position = creditsRect.anchoredPosition;
            position.y += CreditsSpeed * Time.unscaledDeltaTime;

            if (position.y > CreditsEndY)
            {
                position.y = CreditsStartY;
            }

            creditsRect.anchoredPosition = position;
        }
    }
}
