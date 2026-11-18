using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Heraldic enamel colours of menu buttons, chosen by the meaning of the action.
    /// </summary>
    public static class ButtonColors
    {
        public static Color Bronze { get; } = new Color(0.5f, 0.3f, 0.16f, 1f);

        public static Color Gold { get; } = new Color(0.66f, 0.47f, 0.16f, 1f);

        public static Color Emerald { get; } = new Color(0.22f, 0.4f, 0.2f, 1f);

        public static Color Sapphire { get; } = new Color(0.19f, 0.29f, 0.5f, 1f);

        public static Color Amethyst { get; } = new Color(0.37f, 0.22f, 0.42f, 1f);

        public static Color Teal { get; } = new Color(0.18f, 0.37f, 0.37f, 1f);

        public static Color Crimson { get; } = new Color(0.5f, 0.13f, 0.12f, 1f);

        public static Color Lighten(Color color, float amount)
        {
            return Color.Lerp(color, Color.white, amount);
        }

        public static Color Darken(Color color, float amount)
        {
            Color result = Color.Lerp(color, Color.black, amount);
            result.a = color.a;

            return result;
        }
    }
}
