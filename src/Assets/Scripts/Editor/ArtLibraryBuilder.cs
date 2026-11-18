using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Creates the Resources/UiArtLibrary asset with all sprites used by the UI.
    /// </summary>
    public static class ArtLibraryBuilder
    {
        private const int IdleFrameCount = 6;

        public static void Build()
        {
            EditorAssets.EnsureFolder("Assets/Resources");

            UiArtLibrary library = AssetDatabase.LoadAssetAtPath<UiArtLibrary>(GameAssetPaths.ArtLibraryAsset);

            if (library == null)
            {
                library = ScriptableObject.CreateInstance<UiArtLibrary>();
                AssetDatabase.CreateAsset(library, GameAssetPaths.ArtLibraryAsset);
            }

            library.Heart = EditorAssets.LoadArt("UI/Heart");
            library.Pause = EditorAssets.LoadArt("UI/Pause");
            library.Jump = EditorAssets.LoadArt("UI/Jump");
            library.Attack = EditorAssets.LoadArt("UI/Attack");
            library.Coin = EditorAssets.LoadArt("Items/Coin");
            library.Potion = EditorAssets.LoadArt("Items/HealthPotion");
            library.Chest = EditorAssets.LoadArt("Items/Chest");
            library.Sword = EditorAssets.LoadArt("Weapons/Sword");
            library.Piter = EditorAssets.LoadArt("Characters/Piter");
            library.Aleksandra = EditorAssets.LoadArt("Characters/Aleksandra");
            library.Malachor = EditorAssets.LoadArt("Characters/Malachor");

            library.Portraits = new[]
            {
                new SpeakerPortrait("Piter", library.Piter),
                new SpeakerPortrait("Aleksandra", library.Aleksandra),
                new SpeakerPortrait("Malachor", library.Malachor),
                new SpeakerPortrait("Gate Guardian", EditorAssets.LoadArt("Characters/Golem")),
                new SpeakerPortrait("Young Dragon", EditorAssets.LoadArt("Characters/Dragon")),
                new SpeakerPortrait("Royal Guard", EditorAssets.LoadArt("Characters/Skeleton"))
            };

            List<Sprite> previews = new List<Sprite>();
            IReadOnlyList<LevelDefinition> levels = LevelCatalog.Levels;

            for (int index = 0; index < levels.Count; index++)
            {
                previews.Add(EditorAssets.LoadSprite(GameAssetPaths.BackgroundPreview(index, levels[index].Slug)));
            }

            library.LevelPreviews = previews.ToArray();
            library.IdleAnimations = BuildIdleAnimations();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
        }

        private static CharacterFrameSet[] BuildIdleAnimations()
        {
            List<CharacterFrameSet> sets = new List<CharacterFrameSet>();

            foreach (string actor in ActorCatalog.AllActors)
            {
                Sprite[] frames = new Sprite[IdleFrameCount];

                for (int frame = 0; frame < IdleFrameCount; frame++)
                {
                    frames[frame] = EditorAssets.LoadSprite(GameAssetPaths.AnimationFrame(actor, SpriteFrameAnimator.Idle, frame));
                }

                sets.Add(new CharacterFrameSet(actor, frames));
            }

            return sets.ToArray();
        }
    }
}
