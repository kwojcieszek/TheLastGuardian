using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Sprites used by the user interface, loaded from Resources/UiArtLibrary.
    /// </summary>
    public sealed class UiArtLibrary : ScriptableObject
    {
        private const string ResourcePath = "UiArtLibrary";

        private static UiArtLibrary instance;

        [field: SerializeField] public Sprite Heart { get; set; }

        [field: SerializeField] public Sprite Coin { get; set; }

        [field: SerializeField] public Sprite Potion { get; set; }

        [field: SerializeField] public Sprite Pause { get; set; }

        [field: SerializeField] public Sprite Jump { get; set; }

        [field: SerializeField] public Sprite Attack { get; set; }

        [field: SerializeField] public Sprite Chest { get; set; }

        [field: SerializeField] public Sprite Sword { get; set; }

        [field: SerializeField] public Sprite Piter { get; set; }

        [field: SerializeField] public Sprite Aleksandra { get; set; }

        [field: SerializeField] public Sprite Malachor { get; set; }

        [field: SerializeField] public SpeakerPortrait[] Portraits { get; set; } = Array.Empty<SpeakerPortrait>();

        [field: SerializeField] public Sprite[] LevelPreviews { get; set; } = Array.Empty<Sprite>();

        [field: SerializeField] public CharacterFrameSet[] IdleAnimations { get; set; } = Array.Empty<CharacterFrameSet>();

        public static UiArtLibrary Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<UiArtLibrary>(ResourcePath);
                }

                if (instance == null)
                {
                    Debug.LogWarning("Missing Resources/UiArtLibrary asset. Run The Last Guardian > Build Everything.");
                    instance = CreateInstance<UiArtLibrary>();
                }

                return instance;
            }
        }

        public Sprite GetPortrait(string speaker)
        {
            foreach (SpeakerPortrait portrait in Portraits)
            {
                if (string.Equals(portrait.Speaker, speaker, StringComparison.OrdinalIgnoreCase))
                {
                    return portrait.Sprite;
                }
            }

            return null;
        }

        public Sprite[] GetIdleFrames(string character)
        {
            foreach (CharacterFrameSet set in IdleAnimations)
            {
                if (string.Equals(set.Character, character, StringComparison.OrdinalIgnoreCase))
                {
                    return set.Frames;
                }
            }

            return Array.Empty<Sprite>();
        }

        public Sprite GetFace(string character)
        {
            Sprite[] frames = GetIdleFrames(character);

            return frames.Length > 0 ? frames[0] : GetPortrait(character);
        }

        public Sprite GetLevelPreview(int levelId)
        {
            int index = levelId - 1;

            return index >= 0 && index < LevelPreviews.Length ? LevelPreviews[index] : null;
        }
    }
}
