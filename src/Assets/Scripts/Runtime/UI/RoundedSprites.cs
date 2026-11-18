using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Generates anti-aliased rounded rectangle sprites (9-sliced) at runtime.
    /// </summary>
    public static class RoundedSprites
    {
        private const int TextureSize = 128;
        private const int Radius = 30;
        private const float PixelsPerUnit = 100f;
        private const float GradientTop = 1f;
        private const float GradientBottom = 0.76f;
        private const float GlossHeight = 0.42f;
        private const float GlossStrength = 0.08f;

        private static Sprite flat;
        private static Sprite gradient;

        public static Sprite Flat
        {
            get
            {
                if (flat == null)
                {
                    flat = Create("RoundedFlat", false);
                }

                return flat;
            }
        }

        public static Sprite Gradient
        {
            get
            {
                if (gradient == null)
                {
                    gradient = Create("RoundedGradient", true);
                }

                return gradient;
            }
        }

        private static Sprite Create(string name, bool shaded)
        {
            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            texture.name = name;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.DontSave;

            Color32[] pixels = new Color32[TextureSize * TextureSize];

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float alpha = CoverageAt(x, y);
                    float shade = shaded ? ShadeAt(y) : 1f;
                    byte value = (byte)Mathf.RoundToInt(Mathf.Clamp01(shade) * 255f);

                    pixels[y * TextureSize + x] = new Color32(value, value, value, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            Vector4 border = new Vector4(Radius + 2, Radius + 2, Radius + 2, Radius + 2);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect, border);
            sprite.name = name;
            sprite.hideFlags = HideFlags.DontSave;

            return sprite;
        }

        private static float CoverageAt(int x, int y)
        {
            float px = x + 0.5f;
            float py = y + 0.5f;
            float cx = Mathf.Clamp(px, Radius, TextureSize - Radius);
            float cy = Mathf.Clamp(py, Radius, TextureSize - Radius);
            float distance = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));

            return Mathf.Clamp01(Radius - distance + 0.5f);
        }

        private static float ShadeAt(int y)
        {
            float height = (float)y / (TextureSize - 1);
            float shade = Mathf.Lerp(GradientBottom, GradientTop, height);

            if (height > 1f - GlossHeight)
            {
                shade += GlossStrength * Mathf.InverseLerp(1f - GlossHeight, 1f, height);
            }

            return shade;
        }
    }
}
