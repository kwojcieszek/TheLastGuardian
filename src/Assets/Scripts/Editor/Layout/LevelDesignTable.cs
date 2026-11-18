using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Per-level design decisions taken from the story and gameplay documents.
    /// </summary>
    public static class LevelDesignTable
    {
        private static readonly DecorationStyle Crate = new DecorationStyle("Tiles/Wood_64", new Vector2(0.85f, 0.85f), new Color(0.78f, 0.66f, 0.52f, 1f), true);
        private static readonly DecorationStyle Barrel = new DecorationStyle("Items/Barrel", new Vector2(0.8f, 0.95f), new Color(0.82f, 0.74f, 0.64f, 1f), true);
        private static readonly DecorationStyle Torch = new DecorationStyle("Items/Torch", new Vector2(0.45f, 1.5f), Color.white, false);
        private static readonly DecorationStyle Crystal = new DecorationStyle("Items/Crystal", new Vector2(0.55f, 1f), new Color(0.7f, 0.85f, 1f, 0.85f), false);
        private static readonly DecorationStyle Lever = new DecorationStyle("Items/Lever", new Vector2(0.75f, 0.85f), new Color(0.8f, 0.75f, 0.7f, 1f), false);

        public static string[] Terrain { get; } = new[] { "Grass", "Grass", "Stone", "Mine", "Stone", "Castle", "Lava", "Castle", "Castle", "Castle" };

        public static string[][] EnemyPools { get; } = new[]
        {
            new[] { ActorCatalog.Goblin, ActorCatalog.Wolf },
            new[] { ActorCatalog.Goblin, ActorCatalog.Wolf, ActorCatalog.Harpy },
            new[] { ActorCatalog.Skeleton, ActorCatalog.Harpy },
            new[] { ActorCatalog.Goblin, ActorCatalog.Golem },
            new[] { ActorCatalog.Harpy, ActorCatalog.Wolf },
            new[] { ActorCatalog.Skeleton, ActorCatalog.Golem },
            new[] { ActorCatalog.Dragon, ActorCatalog.Harpy },
            new[] { ActorCatalog.Skeleton, ActorCatalog.Golem },
            new[] { ActorCatalog.Harpy, ActorCatalog.Skeleton, ActorCatalog.Golem },
            new[] { ActorCatalog.Skeleton, ActorCatalog.Golem }
        };

        public static DecorationStyle[] GetDecorations(int levelIndex)
        {
            switch (levelIndex)
            {
                case 0:
                    return new[] { Crate, Barrel, Barrel };
                case 1:
                    return new[] { Crate, Barrel };
                case 2:
                    return new[] { Torch, Crate, Barrel };
                case 3:
                    return new[] { Torch, Crystal, Barrel, Crate, Lever };
                case 4:
                    return new[] { Crystal, Crate, Barrel };
                case 5:
                    return new[] { Torch, Barrel, Crate };
                case 6:
                    return new[] { Crystal, Torch };
                case 7:
                    return new[] { Torch, Crate, Lever };
                default:
                    return new[] { Torch, Crystal };
            }
        }

        public static LevelObjectiveConfig GetObjective(int levelId)
        {
            switch (levelId)
            {
                case 2:
                    return new LevelObjectiveConfig { Label = "Silver Leaf", Count = 1, Sprite = "Items/Crystal", Tint = new Color(0.8f, 1f, 0.85f, 1f) };
                case 3:
                    return new LevelObjectiveConfig { Label = "Levers", Count = 3, Sprite = "Items/Lever", Size = new Vector2(1f, 1f), HideWhenCollected = false, Floating = false };
                case 4:
                    return new LevelObjectiveConfig { Label = "Echo Crystal", Count = 1, Sprite = "Items/Crystal", Tint = new Color(0.5f, 0.9f, 1f, 1f) };
                case 5:
                    return new LevelObjectiveConfig { Label = "Castle Map", Count = 1, Sprite = "Items/Key", Tint = new Color(1f, 0.95f, 0.7f, 1f), Size = new Vector2(0.6f, 1f) };
                case 7:
                    return new LevelObjectiveConfig { Label = "Corrupted eggs destroyed", Count = 3, Sprite = "Items/Crystal", Tint = new Color(1f, 0.4f, 0.35f, 1f), RequiresAttack = true, Floating = false };
                case 8:
                    return new LevelObjectiveConfig { Label = "Seals of Light", Count = 3, Sprite = "Items/Crystal", Tint = new Color(1f, 0.92f, 0.45f, 1f) };
                case 9:
                    return new LevelObjectiveConfig { Label = "Prison crystals destroyed", Count = 4, Sprite = "Items/Crystal", Size = new Vector2(0.9f, 1.3f), RequiresAttack = true, Floating = false };
                default:
                    return null;
            }
        }

        public static LayoutNpc[] GetStoryScene(int levelId, float length)
        {
            Color spirit = new Color(0.7f, 1f, 0.95f, 0.65f);
            Color vision = new Color(0.65f, 0.8f, 1f, 0.6f);

            switch (levelId)
            {
                case 1:
                    return new[]
                    {
                        new LayoutNpc { Actor = ActorCatalog.Aleksandra, X = 15f, Speaker = "Aleksandra", Line = "Piter! Help me!", VanishAfterLine = true },
                        new LayoutNpc { Actor = ActorCatalog.Malachor, X = 17.2f, Speaker = "Malachor", Line = "Your sister carries a light that belongs to me!", VanishAfterLine = true, Scale = 1.1f }
                    };
                case 2:
                    return new[] { new LayoutNpc { Actor = ActorCatalog.Harpy, X = 22f, HeightAboveGround = 2.4f, Speaker = "Forest Spirit", Line = "Follow the silver leaves. The shadows cannot see their glow.", Tint = spirit, Floating = true, VanishAfterLine = true } };
                case 3:
                    return new[] { new LayoutNpc { Actor = ActorCatalog.Skeleton, X = 22f, Speaker = "Royal Guard", Line = "Malachor has raised the soldiers of old.", Tint = new Color(1f, 0.9f, 0.6f, 0.75f), VanishAfterLine = true } };
                case 4:
                    return new[] { new LayoutNpc { Actor = ActorCatalog.Aleksandra, X = length * 0.5f + 4f, HeightAboveGround = 0.5f, Speaker = "Aleksandra", Line = "Piter... if you can hear me, do not give up.", Tint = vision, Floating = true, VanishAfterLine = true } };
                case 5:
                    return new[] { new LayoutNpc { Actor = ActorCatalog.Aleksandra, X = length * 0.33f + 8f, HeightAboveGround = 0.5f, Speaker = "Aleksandra", Line = "Look for the sun symbol by the old bridge.", Tint = vision, Floating = true, VanishAfterLine = true } };
                case 7:
                    return new[] { new LayoutNpc { Actor = ActorCatalog.Dragon, X = 26f, HeightAboveGround = 3.2f, Speaker = "Young Dragon", Line = "Grrrr...", Tint = new Color(1f, 0.85f, 0.7f, 0.85f), Floating = true, VanishAfterLine = true, Scale = 0.7f } };
                case 8:
                    return new[] { new LayoutNpc { Actor = ActorCatalog.Harpy, X = 22f, HeightAboveGround = 2.4f, Speaker = "Forest Spirit", Line = "Three seals will weaken Malachor's spell.", Tint = spirit, Floating = true, VanishAfterLine = true } };
                case 9:
                    return new[] { new LayoutNpc { Actor = ActorCatalog.Aleksandra, X = length - 9f, HeightAboveGround = 0.6f, Speaker = "Aleksandra", Line = "Piter! The barrier is too strong!", Tint = new Color(0.85f, 0.7f, 1f, 0.85f), Floating = true } };
                default:
                    return new LayoutNpc[0];
            }
        }

        public static GuardianConfig GetGuardian(int levelId)
        {
            switch (levelId)
            {
                case 3:
                    return new GuardianConfig { Actor = ActorCatalog.Skeleton, DisplayName = "the Bone Captain", MaxHealth = 6, Scale = 1.3f };
                case 6:
                    return new GuardianConfig { Actor = ActorCatalog.Golem, DisplayName = "the Gate Guardian", MaxHealth = 10, Scale = 1.25f };
                case 7:
                    return new GuardianConfig { Actor = ActorCatalog.Dragon, DisplayName = "the Young Dragon", MaxHealth = 8, Scale = 1f };
                case 10:
                    return new GuardianConfig { Actor = ActorCatalog.Malachor, DisplayName = "Malachor", MaxHealth = 25, Scale = 1.2f };
                default:
                    return null;
            }
        }
    }
}
