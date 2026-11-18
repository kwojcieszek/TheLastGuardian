using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Plans long rightward levels from seven modular obstacle patterns.
    /// </summary>
    public static class LevelLayoutPlanner
    {
        public const float GroundTop = -1.5f;
        public const float SectionWidth = 40f;
        public const float SpawnY = -0.65f;
        public const float FlyerHeight = 1.1f;

        private const float GroundDepth = 3.5f;
        private const float GuardianDistanceFromEnd = 22f;
        private const float PotionOffset = 5f;
        private const float ObjectiveOffset = 9f;
        private const float PrimaryEnemyOffset = 36f;
        private const float SecondaryEnemyOffset = 20f;
        private const float SecondaryFlyerLift = 0.8f;
        private const float DecorationMinStep = 3.2f;
        private const float DecorationMaxStep = 6.5f;
        private const float DecorationClearance = 1.6f;
        private const float DecorationStart = 7f;
        private const float DecorationEndMargin = 9f;
        private const double StackChance = 0.25;

        private static readonly string[] Kinds = { "Hurdles", "DoubleGap", "HighRoute", "Spikes", "MovingBridge", "TimedTrap", "SteppingIslands" };
        private static readonly float[] PotionFractions = { 0.22f, 0.5f, 0.8f };
        private static readonly int[] OpenGroundPatterns = { 0, 3, 5 };

        public static LevelLayout Plan(LevelDefinition definition, int index)
        {
            LevelLayout layout = new LevelLayout
            {
                Id = definition.Id,
                Index = index,
                Name = definition.Name,
                Biome = definition.Slug,
                Scene = definition.Scene,
                Length = definition.LengthMeters,
                Objective = LevelDesignTable.GetObjective(definition.Id),
                Guardian = LevelDesignTable.GetGuardian(definition.Id)
            };

            layout.Checkpoints = CreateCheckpoints(layout.Length);

            PlanSections(layout);

            HashSet<int> chestSections = ChooseChestSections(layout);

            for (int section = 0; section < layout.Sections.Count; section++)
            {
                BuildSection(layout, section, chestSections.Contains(section));
            }

            PlacePotions(layout);
            PlaceObjectives(layout);
            PlaceGuardian(layout);
            PlaceDecorations(layout);
            layout.Npcs.AddRange(LevelDesignTable.GetStoryScene(layout.Id, layout.Length));

            return layout;
        }

        private static float[] CreateCheckpoints(float length)
        {
            return new[]
            {
                4f,
                Mathf.Floor(length * 0.33f / SectionWidth) * SectionWidth + 4f,
                Mathf.Floor(length * 0.68f / SectionWidth) * SectionWidth + 4f
            };
        }

        private static void PlanSections(LevelLayout layout)
        {
            int count = Mathf.CeilToInt(layout.Length / SectionWidth);

            for (int section = 0; section < count; section++)
            {
                float x = section * SectionWidth;
                float end = Mathf.Min(layout.Length, x + SectionWidth);
                bool arena = layout.Guardian != null && section == count - 2;
                bool safe = section == 0 || section == count - 1 || arena || Array.Exists(layout.Checkpoints, point => point >= x && point < end);
                int pattern = (section + layout.Id * 2) % Kinds.Length;

                if (layout.Id == 1 && section == 1)
                {
                    pattern = 0;
                }

                if (layout.Id == 1 && section == 2)
                {
                    pattern = 1;
                }

                layout.Sections.Add(new LayoutSection
                {
                    X = x,
                    Width = end - x,
                    Pattern = safe ? -1 : pattern,
                    Safe = safe,
                    Kind = safe ? "Safe" : Kinds[pattern]
                });
            }
        }

        private static HashSet<int> ChooseChestSections(LevelLayout layout)
        {
            List<int> highRoutes = new List<int>();

            for (int section = 0; section < layout.Sections.Count; section++)
            {
                if (layout.Sections[section].Pattern == 2)
                {
                    highRoutes.Add(section);
                }
            }

            HashSet<int> chosen = new HashSet<int>();

            if (highRoutes.Count >= 2)
            {
                chosen.Add(highRoutes[highRoutes.Count / 3]);
                chosen.Add(highRoutes[Mathf.Min(highRoutes.Count - 1, highRoutes.Count * 2 / 3 + 1)]);
            }
            else if (highRoutes.Count == 1)
            {
                chosen.Add(highRoutes[0]);
            }

            return chosen;
        }

        private static void BuildSection(LevelLayout layout, int index, bool chest)
        {
            LayoutSection section = layout.Sections[index];
            float x = section.X;
            float end = section.End;
            bool barrels = layout.Id <= 2;

            switch (section.Pattern)
            {
                case 0:
                    AddGround(layout, x, end);
                    AddBarrel(layout, x + 12f, 1.5f, 0.9f);
                    AddPlatform(layout, barrels ? "Barrel" : "Block", x + 25f, GroundTop + 1.4f, 1.5f, 1.4f);
                    AddBarrel(layout, x + 30f, 0.8f, 1f);
                    break;
                case 1:
                    AddGround(layout, x, x + 14f);
                    AddBarrel(layout, x + 9f, 0.8f, 1f);
                    AddGround(layout, x + 18f, x + 28f);
                    AddGround(layout, x + 32f, end);
                    break;
                case 2:
                    AddGround(layout, x, end);
                    AddPlatform(layout, "Step", x + 12f, 0f, 3.5f);
                    AddPlatform(layout, "Step", x + 17f, 1.2f, 3.5f);
                    AddPlatform(layout, "Step", x + 22f, 2.4f, 3.5f);
                    AddPickup(layout, chest ? PickupKind.Chest : PickupKind.Coin, new Vector2(x + 22f, chest ? 2.85f : 3.3f));
                    AddBarrel(layout, x + 30f, 0.8f, 1f);
                    break;
                case 3:
                    AddGround(layout, x, end);
                    AddBarrel(layout, x + 4.5f, 0.8f, 1f);
                    AddHazard(layout, "Spikes", new Vector2(x + 15f, GroundTop + 0.3f), new Vector2(2.5f, 0.6f), Vector2.zero);
                    AddHazard(layout, "Spikes", new Vector2(x + 28f, GroundTop + 0.3f), new Vector2(2.5f, 0.6f), Vector2.zero);
                    break;
                case 4:
                    AddGround(layout, x, x + 14f);
                    AddGround(layout, x + 21f, end);
                    AddHazard(layout, "Spikes", new Vector2(x + 29f, GroundTop + 0.3f), new Vector2(2f, 0.6f), Vector2.zero);
                    layout.Surfaces.Add(new LayoutSurface
                    {
                        Kind = "Moving",
                        Position = new Vector2(x + 14.5f, -1.25f),
                        Destination = new Vector2(x + 20.5f, -1.25f),
                        Size = new Vector2(3f, 0.5f)
                    });
                    break;
                case 5:
                    AddGround(layout, x, end);
                    AddPlatform(layout, "Block", x + 12f, -0.4f, 2f, 1.1f);
                    AddHazard(layout, "MovingCrystal", new Vector2(x + 25f, -0.9f), new Vector2(0.9f, 0.9f), new Vector2(x + 25f, 2.8f));
                    AddBarrel(layout, x + 31f, 1.5f, 0.9f);
                    break;
                case 6:
                    AddGround(layout, x, x + 12f);
                    AddGround(layout, x + 28f, end);
                    AddPlatform(layout, "Island", x + 16f, -0.5f, 4f);
                    AddPlatform(layout, "Island", x + 23f, 0.6f, 4f);
                    AddBarrel(layout, x + 32f, 0.8f, 1f);
                    break;
                default:
                    AddGround(layout, x, end);
                    break;
            }

            AddSectionEnemy(layout, index);
            AddSectionCoins(layout, section);
        }

        private static void AddSectionEnemy(LevelLayout layout, int index)
        {
            LayoutSection section = layout.Sections[index];
            bool tutorial = layout.Id == 1;

            if (section.Safe || (tutorial && index % 2 != 1))
            {
                return;
            }

            string[] pool = LevelDesignTable.EnemyPools[layout.Index];
            AddEnemy(layout, pool[index % pool.Length], section.X + PrimaryEnemyOffset, 0f);

            if (tutorial || index % 2 == 0)
            {
                return;
            }

            string second = pool[(index + 1) % pool.Length];
            bool flying = ActorCatalog.Get(second).IsFlying;
            bool openGround = Array.IndexOf(OpenGroundPatterns, section.Pattern) >= 0;

            if (flying || openGround)
            {
                AddEnemy(layout, second, section.X + SecondaryEnemyOffset, flying ? SecondaryFlyerLift : 0f);
            }
        }

        private static void AddEnemy(LevelLayout layout, string enemy, float x, float lift)
        {
            ActorProfile profile = ActorCatalog.Get(enemy);
            float rootY = profile.IsFlying ? FlyerHeight + lift : profile.GroundedRootY(GroundTop);

            layout.Enemies.Add(new LayoutEnemy { Kind = enemy, Position = new Vector2(x, rootY) });
        }

        private static void AddSectionCoins(LevelLayout layout, LayoutSection section)
        {
            if (section.Width < 28f)
            {
                return;
            }

            AddPickup(layout, PickupKind.Coin, new Vector2(section.X + 8f, -0.55f));
            AddPickup(layout, PickupKind.Coin, new Vector2(section.X + 34f, -0.55f));

            if (section.Pattern == 1 || section.Pattern == 4 || section.Pattern == 6)
            {
                AddPickup(layout, PickupKind.Coin, new Vector2(section.X + 16f, 0.8f));
            }
        }

        private static void PlacePotions(LevelLayout layout)
        {
            foreach (float fraction in PotionFractions)
            {
                LayoutSection section = FindPlayableSection(layout, fraction * layout.Length);
                AddPickup(layout, PickupKind.Potion, new Vector2(section.X + PotionOffset, GroundTop + 0.5f));
            }
        }

        private static void PlaceObjectives(LevelLayout layout)
        {
            LevelObjectiveConfig objective = layout.Objective;

            if (objective == null)
            {
                return;
            }

            float lift = objective.Floating ? 0.35f : 0.02f;

            for (int item = 0; item < objective.Count; item++)
            {
                float fraction = 0.9f * (item + 1) / (objective.Count + 1);
                LayoutSection section = FindPlayableSection(layout, fraction * layout.Length);
                Vector2 position = new Vector2(section.X + ObjectiveOffset, GroundTop + objective.Size.y * 0.5f + lift);

                AddPickup(layout, PickupKind.Objective, position);
            }
        }

        private static void PlaceGuardian(LevelLayout layout)
        {
            GuardianConfig guardian = layout.Guardian;

            if (guardian == null)
            {
                return;
            }

            ActorProfile profile = ActorCatalog.Get(guardian.Actor);
            float y = profile.IsFlying ? FlyerHeight + 0.4f : GroundTop + profile.FeetOffset * guardian.Scale + 0.05f;

            layout.Enemies.Add(new LayoutEnemy
            {
                Kind = guardian.Actor,
                Position = new Vector2(layout.Length - GuardianDistanceFromEnd, y),
                IsGuardian = true
            });
        }

        private static void PlaceDecorations(LevelLayout layout)
        {
            DecorationStyle[] styles = LevelDesignTable.GetDecorations(layout.Index);
            System.Random random = new System.Random(layout.Id * 7919);
            float x = DecorationStart;

            while (x < layout.Length - DecorationEndMargin)
            {
                if (IsFreeGround(layout, x))
                {
                    DecorationStyle style = styles[random.Next(styles.Length)];
                    float scale = 0.85f + (float)random.NextDouble() * 0.3f;
                    bool mirrored = random.Next(2) == 0;

                    AddDecoration(layout, style, x, GroundTop, scale, mirrored);

                    if (style.Stackable && random.NextDouble() < StackChance)
                    {
                        AddDecoration(layout, style, x + 0.12f, GroundTop + style.Size.y * scale, scale * 0.8f, mirrored);
                    }
                }

                x += DecorationMinStep + (float)random.NextDouble() * (DecorationMaxStep - DecorationMinStep);
            }
        }

        private static bool IsFreeGround(LevelLayout layout, float x)
        {
            bool onGround = layout.Surfaces.Exists(surface => surface.Kind == "Ground" && Mathf.Abs(surface.Position.x - x) < surface.Size.x * 0.5f - DecorationClearance);

            if (!onGround)
            {
                return false;
            }

            bool blocked = layout.Surfaces.Exists(surface => surface.Kind != "Ground" && Mathf.Abs(surface.Position.x - x) < surface.Size.x * 0.5f + DecorationClearance);
            blocked |= layout.Hazards.Exists(hazard => Mathf.Abs(hazard.Position.x - x) < hazard.Size.x * 0.5f + DecorationClearance);
            blocked |= Array.Exists(layout.Checkpoints, point => Mathf.Abs(point - x) < DecorationClearance);

            return !blocked;
        }

        private static void AddDecoration(LevelLayout layout, DecorationStyle style, float x, float bottom, float scale, bool mirrored)
        {
            Vector2 size = style.Size * scale;

            layout.Decorations.Add(new LayoutDecoration
            {
                Sprite = style.Sprite,
                Position = new Vector2(x, bottom + size.y * 0.5f),
                Size = size,
                Tint = style.Tint,
                Mirrored = mirrored
            });
        }

        private static LayoutSection FindPlayableSection(LevelLayout layout, float x)
        {
            int start = Mathf.Clamp(Mathf.FloorToInt(x / SectionWidth), 1, layout.Sections.Count - 1);

            for (int index = start; index < layout.Sections.Count - 1; index++)
            {
                if (!layout.Sections[index].Safe)
                {
                    return layout.Sections[index];
                }
            }

            return layout.Sections[start];
        }

        private static void AddGround(LevelLayout layout, float left, float right)
        {
            layout.Surfaces.Add(new LayoutSurface
            {
                Kind = "Ground",
                Position = new Vector2((left + right) * 0.5f, GroundTop - GroundDepth * 0.5f),
                Size = new Vector2(right - left, GroundDepth)
            });
        }

        private static void AddPlatform(LevelLayout layout, string kind, float x, float top, float width, float height = 0.5f)
        {
            layout.Surfaces.Add(new LayoutSurface
            {
                Kind = kind,
                Position = new Vector2(x, top - height * 0.5f),
                Size = new Vector2(width, height)
            });
        }

        private static void AddBarrel(LevelLayout layout, float x, float width, float height)
        {
            AddPlatform(layout, "Barrel", x, GroundTop + height, width, height);
        }

        private static void AddHazard(LevelLayout layout, string kind, Vector2 position, Vector2 size, Vector2 destination)
        {
            layout.Hazards.Add(new LayoutHazard { Kind = kind, Position = position, Size = size, Destination = destination });
        }

        private static void AddPickup(LevelLayout layout, PickupKind kind, Vector2 position)
        {
            layout.Pickups.Add(new LayoutPickup { Kind = kind, Position = position });
        }
    }
}
