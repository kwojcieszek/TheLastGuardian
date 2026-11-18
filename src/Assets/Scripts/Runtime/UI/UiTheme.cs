using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Medieval colour palette shared by all screens: aged brass, dark oak and parchment.
    /// </summary>
    public static class UiTheme
    {
        public static Color Gold { get; } = new Color(0.93f, 0.76f, 0.4f, 1f);

        public static Color TextColor { get; } = new Color(0.97f, 0.92f, 0.8f, 1f);

        public static Color MutedText { get; } = new Color(0.78f, 0.7f, 0.58f, 1f);

        public static Color FrameColor { get; } = new Color(0.72f, 0.54f, 0.27f, 1f);

        public static Color FrameEdge { get; } = new Color(0.17f, 0.1f, 0.05f, 1f);

        public static Color PanelColor { get; } = new Color(0.14f, 0.09f, 0.055f, 0.97f);

        public static Color OverlayColor { get; } = new Color(0.03f, 0.02f, 0.01f, 0.62f);

        public static Color ButtonDisabled { get; } = new Color(0.25f, 0.22f, 0.19f, 0.8f);

        public static Color BarBackground { get; } = new Color(0.06f, 0.035f, 0.02f, 0.7f);

        public static Color ProgressFill { get; } = new Color(0.86f, 0.66f, 0.3f, 1f);

        public static Color BossFill { get; } = new Color(0.58f, 0.12f, 0.14f, 1f);

        public static Color LostHeart { get; } = new Color(0.2f, 0.18f, 0.16f, 0.55f);

        public static Color TouchButtonColor { get; } = new Color(1f, 0.95f, 0.85f, 0.55f);

        public static Color TouchButtonPressed { get; } = new Color(1f, 0.82f, 0.5f, 0.9f);
    }
}
