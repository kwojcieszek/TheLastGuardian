using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Grid of the ten campaign stages with lock state and best results.
    /// </summary>
    public sealed class LevelSelectView : MonoBehaviour
    {
        private const int Columns = 5;

        private static readonly Vector2 CardSize = new Vector2(330f, 300f);
        private static readonly Color LockedTint = new Color(0.25f, 0.25f, 0.25f, 1f);

        private readonly List<Button> cards = new List<Button>();
        private readonly List<Text> statusTexts = new List<Text>();
        private readonly List<Image> previews = new List<Image>();
        private Button backButton;

        public bool IsOpen => gameObject.activeSelf;

        public event Action<int> LevelChosen;

        public event Action Closed;

        public static LevelSelectView Create(Transform parent)
        {
            RectTransform root = MenuPanel.CreateRoot(parent, "LevelSelect", true);

            LevelSelectView view = root.gameObject.AddComponent<LevelSelectView>();
            view.Build();
            view.gameObject.SetActive(false);

            return view;
        }

        public void Show()
        {
            Refresh();
            gameObject.SetActive(true);

            UiFactory.Select(cards.Count > 0 ? cards[0] : backButton);
        }

        public void Close()
        {
            gameObject.SetActive(false);

            Closed?.Invoke();
        }

        private static void AddEnemyFaces(Transform card, LevelDefinition level)
        {
            UiArtLibrary art = UiArtLibrary.Instance;
            RectTransform row = UiFactory.CreateRect("Enemies", card);
            UiFactory.Place(row, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(300f, 76f));
            UiFactory.AddHorizontalLayout(row, -6f, TextAnchor.MiddleRight);

            int index = 0;

            foreach (string enemy in level.EnemyTypes)
            {
                string character = MatchCharacter(art, enemy);

                if (character == null)
                {
                    continue;
                }

                Image face = UiFactory.CreateImage(row, character, art.GetFace(character), new Vector2(84f, 84f));
                face.rectTransform.localScale = new Vector3(-1f, 1f, 1f);

                UiSpriteAnimator animator = face.gameObject.AddComponent<UiSpriteAnimator>();
                animator.Play(art.GetIdleFrames(character), 7f, index * 0.37f);
                index++;
            }
        }

        private static string MatchCharacter(UiArtLibrary art, string enemyType)
        {
            foreach (CharacterFrameSet set in art.IdleAnimations)
            {
                if (enemyType.IndexOf(set.Character, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return set.Character;
                }
            }

            return null;
        }

        private static string DescribeStatus(bool unlocked, LevelRecord record, SaveData save, int levelId)
        {
            if (!unlocked)
            {
                return "Locked";
            }

            if (record != null && record.Completed)
            {
                return $"Completed  |  Coins {record.BestCoins}  |  {UiFactory.FormatTime(record.BestTimeSeconds)}";
            }

            return save.CheckpointLevel == levelId ? "In progress (checkpoint saved)" : "Not completed";
        }

        private void Build()
        {
            RectTransform window = MenuPanel.CreateWindow(transform, "Select Stage", new Vector2(1820f, 940f));

            RectTransform grid = UiFactory.CreateRect("Grid", window);
            UiFactory.Place(grid, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(1760f, 640f));

            GridLayoutGroup layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = CardSize;
            layout.spacing = new Vector2(16f, 24f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = Columns;
            layout.childAlignment = TextAnchor.UpperCenter;

            foreach (LevelDefinition level in LevelCatalog.Levels)
            {
                CreateCard(grid, level);
            }

            backButton = UiFactory.CreateButton(window, "Back", Close, new Vector2(420f, 90f), ButtonColors.Crimson);
            UiFactory.Place((RectTransform)backButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(420f, 90f));
        }

        private void CreateCard(Transform grid, LevelDefinition level)
        {
            int levelId = level.Id;
            Button card = UiFactory.CreateButton(grid, string.Empty, () => LevelChosen?.Invoke(levelId), CardSize, 26);

            Image preview = UiFactory.CreateImage(card.transform, "Preview", UiArtLibrary.Instance.GetLevelPreview(levelId), new Vector2(310f, 175f));
            UiFactory.Place(preview.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(310f, 175f));
            preview.preserveAspect = false;

            AddEnemyFaces(card.transform, level);

            Text title = UiFactory.CreateText(card.transform, "Title", level.Title, 26, UiTheme.Gold, TextAnchor.MiddleCenter);
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(320f, 50f));

            Text status = UiFactory.CreateText(card.transform, "Status", string.Empty, 22, UiTheme.TextColor, TextAnchor.MiddleCenter);
            UiFactory.Place(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(320f, 46f));

            cards.Add(card);
            previews.Add(preview);
            statusTexts.Add(status);
        }

        private void Refresh()
        {
            SaveData save = SaveSystem.Current;

            for (int index = 0; index < cards.Count; index++)
            {
                int levelId = index + 1;
                bool unlocked = save.IsUnlocked(levelId);
                LevelRecord record = save.FindRecord(levelId);

                cards[index].interactable = unlocked;
                previews[index].color = unlocked ? Color.white : LockedTint;
                statusTexts[index].text = DescribeStatus(unlocked, record, save, levelId);
            }
        }

    }
}
