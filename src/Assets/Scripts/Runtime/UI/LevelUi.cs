using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Creates and owns every screen used inside a level.
    /// </summary>
    public sealed class LevelUi
    {
        public HudView Hud { get; }

        public TouchControlsView TouchControls { get; }

        public DialogueView Dialogue { get; }

        public PauseMenuView Pause { get; }

        public SettingsView Settings { get; }

        public LevelCompleteView LevelComplete { get; }

        public EndingView Ending { get; }

        public LevelUi(LevelSession session)
        {
            UiFactory.EnsureEventSystem();

            Canvas canvas = UiFactory.CreateCanvas("LevelUI", 10);
            Transform root = canvas.transform;

            RectTransform safeArea = UiFactory.CreateRect("SafeArea", root);
            UiFactory.Stretch(safeArea);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            Hud = HudView.Create(safeArea, session);
            TouchControls = TouchControlsView.Create(safeArea, session);
            Dialogue = DialogueView.Create(root);
            Pause = PauseMenuView.Create(root);
            Settings = SettingsView.Create(root);
            LevelComplete = LevelCompleteView.Create(root);
            Ending = EndingView.Create(root);
        }
    }
}
