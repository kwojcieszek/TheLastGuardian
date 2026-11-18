using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Generates the ten playable level scenes from the planned layouts.
    /// </summary>
    public static class LevelSceneBuilder
    {
        private const float CameraSize = 4.5f;
        private const float CameraFixedY = 1f;
        private const float ExitDistanceFromEnd = 5f;
        private const float AlphaThreshold = 48f;
        private const int DecorationOrder = 1;
        private const int TerrainOrder = 2;
        private const int PropOrder = 5;
        private const int ChestCoinReward = 10;

        private static readonly Dictionary<Sprite, Rect> AlphaRects = new Dictionary<Sprite, Rect>();
        private static readonly float[] ParallaxStrengths = { 0.92f, 0.68f, 0.38f };
        private static readonly Color BackgroundColor = new Color(0.08f, 0.06f, 0.1f, 1f);
        private static readonly Color ExitTint = new Color(1f, 0.9f, 0.55f, 1f);

        public static List<string> BuildAll()
        {
            AlphaRects.Clear();
            EditorAssets.EnsureFolder(GameAssetPaths.LevelScenesRoot);

            List<string> scenePaths = new List<string>();
            IReadOnlyList<LevelDefinition> levels = LevelCatalog.Levels;

            if (levels.Count == 0)
            {
                throw new InvalidOperationException("No levels found in Resources/Data/levels.json.");
            }

            for (int index = 0; index < levels.Count; index++)
            {
                LevelLayout layout = LevelLayoutPlanner.Plan(levels[index], index);
                string path = GameAssetPaths.LevelScenesRoot + layout.Scene + ".unity";

                BuildScene(layout);
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), path);
                scenePaths.Add(path);

                Debug.Log($"Level built: {layout.Scene} ({layout.Length} units, {layout.Enemies.Count} enemies, {layout.Surfaces.FindAll(surface => surface.Kind == "Barrel").Count} barrels, {layout.Hazards.Count} hazards, {layout.Decorations.Count} decorations, {layout.CountPickups(PickupKind.Coin)} coins)");
            }

            return scenePaths;
        }

        private static void BuildScene(LevelLayout layout)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("Level_" + layout.Id.ToString("00"));
            LevelSession session = root.AddComponent<LevelSession>();

            BuildTerrain(layout, root.transform);
            BuildDecorations(layout, root.transform);
            BuildHazards(layout, root.transform);

            PiterController player = InstantiateActor(ActorCatalog.Piter, new Vector2(4f, LevelLayoutPlanner.SpawnY), root.transform).GetComponent<PiterController>();
            EnemyController guardian = BuildEnemies(layout, root.transform);

            BuildCheckpoints(layout, root.transform);
            int coins = BuildPickups(layout, root.transform);
            BuildExit(layout, root.transform);
            BuildStoryCharacters(layout, root.transform);

            if (layout.Guardian != null && layout.Guardian.Actor == ActorCatalog.Malachor)
            {
                GameObject sister = InstantiateActor(ActorCatalog.Aleksandra, new Vector2(layout.Length - 8f, LevelLayoutPlanner.SpawnY), root.transform);
                sister.GetComponent<SpriteRenderer>().flipX = true;
                RecordOverrides(sister.GetComponent<SpriteRenderer>());
            }

            FollowCamera2D followCamera = BuildCamera(layout, player);
            BuildBackground(layout, followCamera.transform);

            session.LevelId = layout.Id;
            session.LevelLength = layout.Length;
            session.Player = player;
            session.FollowCamera = followCamera;
            session.StartPoint = new Vector2(4f, LevelLayoutPlanner.SpawnY);
            session.Guardian = guardian;
            session.ObjectiveTarget = layout.Objective != null ? layout.Objective.Count : 0;
            session.ObjectiveLabel = layout.Objective != null ? layout.Objective.Label : string.Empty;
            session.TotalChests = layout.CountPickups(PickupKind.Chest);
            session.TotalCoins = coins + session.TotalChests * ChestCoinReward;
        }

        private static void BuildTerrain(LevelLayout layout, Transform root)
        {
            Transform terrain = CreateGroup("Terrain", root);
            int groundLayer = EditorAssets.RequireLayer(GameLayers.GroundName);
            string groundTile = "Tiles/" + LevelDesignTable.Terrain[layout.Index] + "_64";

            foreach (LayoutSurface surface in layout.Surfaces)
            {
                GameObject piece;

                if (surface.Kind == "Barrel")
                {
                    Sprite barrel = EditorAssets.LoadArt("Items/Barrel");
                    piece = CreateVisual("Hurdle_Barrel", barrel, surface.Position, surface.Size, terrain, PropOrder);

                    AddFittedCollider(piece, barrel, surface.Size, false);
                }
                else
                {
                    string tile = surface.Kind == "Ground" ? groundTile : surface.Kind == "Moving" ? "Tiles/Platform_64" : "Tiles/Stone_64";
                    piece = CreateTiled(surface.Kind, EditorAssets.LoadArt(tile), surface.Position, surface.Size, terrain);
                }

                piece.layer = groundLayer;

                if (surface.Kind == "Moving")
                {
                    Rigidbody2D body = piece.AddComponent<Rigidbody2D>();
                    body.bodyType = RigidbodyType2D.Kinematic;

                    MovingPlatform2D mover = piece.AddComponent<MovingPlatform2D>();
                    mover.PointA = surface.Position;
                    mover.PointB = surface.Destination;
                }
            }

            GameObject left = CreateTiled("LeftBoundary", EditorAssets.LoadArt("Tiles/Stone_64"), new Vector2(-0.5f, 0f), new Vector2(1f, 12f), terrain);
            GameObject right = CreateTiled("RightBoundary", EditorAssets.LoadArt("Tiles/Stone_64"), new Vector2(layout.Length + 0.5f, 0f), new Vector2(1f, 12f), terrain);
            left.layer = groundLayer;
            right.layer = groundLayer;
        }

        private static void BuildDecorations(LevelLayout layout, Transform root)
        {
            Transform group = CreateGroup("Decorations", root);

            foreach (LayoutDecoration decoration in layout.Decorations)
            {
                GameObject piece = CreateVisual("Decor", EditorAssets.LoadArt(decoration.Sprite), decoration.Position, decoration.Size, group, DecorationOrder);

                foreach (SpriteRenderer display in piece.GetComponentsInChildren<SpriteRenderer>())
                {
                    display.color = decoration.Tint;
                    display.flipX = decoration.Mirrored;
                }
            }
        }

        private static void BuildHazards(LevelLayout layout, Transform root)
        {
            Transform hazards = CreateGroup("Hazards", root);
            int interactable = EditorAssets.RequireLayer(GameLayers.InteractableName);

            foreach (LayoutHazard hazard in layout.Hazards)
            {
                Sprite sprite = EditorAssets.LoadArt(hazard.Kind == "Spikes" ? "Items/Spikes" : "Items/Crystal");
                GameObject piece = CreateVisual(hazard.Kind, sprite, hazard.Position, hazard.Size, hazards, PropOrder);
                piece.layer = interactable;

                AddFittedCollider(piece, sprite, hazard.Size, true);
                piece.AddComponent<Hazard>();

                if (hazard.Kind == "MovingCrystal")
                {
                    piece.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

                    MovingPlatform2D mover = piece.AddComponent<MovingPlatform2D>();
                    mover.PointA = hazard.Position;
                    mover.PointB = hazard.Destination;
                    mover.Speed = 1.5f;
                }
            }

            GameObject fall = new GameObject("FallZone");
            fall.transform.SetParent(hazards, false);
            fall.transform.position = new Vector2(layout.Length * 0.5f, -8f);
            fall.layer = interactable;

            BoxCollider2D fallTrigger = fall.AddComponent<BoxCollider2D>();
            fallTrigger.size = new Vector2(layout.Length + 40f, 4f);
            fallTrigger.isTrigger = true;

            fall.AddComponent<Hazard>().Lethal = true;
        }

        private static EnemyController BuildEnemies(LevelLayout layout, Transform root)
        {
            Transform enemies = CreateGroup("Enemies", root);
            EnemyController guardian = null;

            foreach (LayoutEnemy spawn in layout.Enemies)
            {
                GameObject enemy = InstantiateActor(spawn.Kind, spawn.Position, enemies);

                if (!spawn.IsGuardian)
                {
                    continue;
                }

                GuardianConfig config = layout.Guardian;
                enemy.name = "Guardian_" + spawn.Kind;
                enemy.transform.localScale = new Vector3(config.Scale, config.Scale, 1f);

                guardian = enemy.GetComponent<EnemyController>();
                guardian.DisplayName = config.DisplayName;
                guardian.IsBoss = true;
                guardian.DetectionDistance = Mathf.Max(guardian.DetectionDistance, 8f);
                guardian.PatrolDistance = Mathf.Max(guardian.PatrolDistance, 4f);

                Health health = enemy.GetComponent<Health>();
                health.MaxHealth = config.MaxHealth;
                health.InvulnerabilitySeconds = 0.35f;

                RecordOverrides(enemy.transform);
                RecordOverrides(guardian);
                RecordOverrides(health);
            }

            return guardian;
        }

        private static void BuildCheckpoints(LevelLayout layout, Transform root)
        {
            Transform group = CreateGroup("Checkpoints", root);
            int interactable = EditorAssets.RequireLayer(GameLayers.InteractableName);
            Sprite flag = EditorAssets.LoadArt("Items/Flag");

            foreach (float x in layout.Checkpoints)
            {
                GameObject checkpoint = CreateVisual("Checkpoint", flag, new Vector2(x, -0.45f), new Vector2(0.85f, 2.1f), group, PropOrder);
                checkpoint.layer = interactable;

                BoxCollider2D trigger = checkpoint.AddComponent<BoxCollider2D>();
                trigger.size = new Vector2(1.4f, 2.5f);
                trigger.isTrigger = true;

                Checkpoint2D component = checkpoint.AddComponent<Checkpoint2D>();
                component.SpawnPoint = new Vector2(x, LevelLayoutPlanner.SpawnY);
                component.FlagRenderer = checkpoint.GetComponentInChildren<SpriteRenderer>();
            }
        }

        private static int BuildPickups(LevelLayout layout, Transform root)
        {
            Transform group = CreateGroup("Pickups", root);
            int interactable = EditorAssets.RequireLayer(GameLayers.InteractableName);
            int coins = 0;

            foreach (LayoutPickup pickup in layout.Pickups)
            {
                GameObject item = CreatePickup(layout, pickup, group);
                item.layer = interactable;

                if (pickup.Kind == PickupKind.Coin)
                {
                    coins++;
                }
            }

            return coins;
        }

        private static GameObject CreatePickup(LevelLayout layout, LayoutPickup pickup, Transform group)
        {
            switch (pickup.Kind)
            {
                case PickupKind.Potion:
                {
                    GameObject potion = CreateVisual("HealthPotion", EditorAssets.LoadArt("Items/HealthPotion"), pickup.Position, new Vector2(0.55f, 0.7f), group, PropOrder);
                    AddBob(potion);

                    CircleCollider2D trigger = potion.AddComponent<CircleCollider2D>();
                    trigger.radius = 0.4f;
                    trigger.isTrigger = true;

                    potion.AddComponent<PotionPickup>();

                    return potion;
                }
                case PickupKind.Chest:
                {
                    GameObject chest = CreateVisual("SecretChest", EditorAssets.LoadArt("Items/Chest"), pickup.Position, new Vector2(1.1f, 0.9f), group, PropOrder);
                    BoxCollider2D trigger = chest.AddComponent<BoxCollider2D>();
                    trigger.size = new Vector2(1.2f, 1f);
                    trigger.isTrigger = true;

                    TreasureChest component = chest.AddComponent<TreasureChest>();
                    component.ChestRenderer = chest.GetComponentInChildren<SpriteRenderer>();
                    component.CoinReward = ChestCoinReward;

                    return chest;
                }
                case PickupKind.Objective:
                    return CreateObjective(layout.Objective, pickup.Position, group);
                default:
                {
                    GameObject coin = CreateVisual("Coin", EditorAssets.LoadArt("Items/Coin"), pickup.Position, new Vector2(0.42f, 0.42f), group, PropOrder);
                    AddBob(coin);

                    CircleCollider2D trigger = coin.AddComponent<CircleCollider2D>();
                    trigger.radius = 0.3f;
                    trigger.isTrigger = true;

                    coin.AddComponent<CoinPickup>();

                    return coin;
                }
            }
        }

        private static GameObject CreateObjective(LevelObjectiveConfig config, Vector2 position, Transform group)
        {
            GameObject item = CreateVisual("Objective_" + config.Label, EditorAssets.LoadArt(config.Sprite), position, config.Size, group, PropOrder + 1);
            SpriteRenderer display = item.GetComponentInChildren<SpriteRenderer>();
            display.color = config.Tint;

            if (config.Floating)
            {
                AddBob(item);
            }

            BoxCollider2D trigger = item.AddComponent<BoxCollider2D>();
            trigger.size = config.Size + new Vector2(0.3f, 0.3f);
            trigger.isTrigger = true;

            ObjectiveItem objective = item.AddComponent<ObjectiveItem>();
            objective.RequiresAttack = config.RequiresAttack;
            objective.HideWhenCollected = config.HideWhenCollected;
            objective.ItemRenderer = display;

            return item;
        }

        private static void BuildExit(LevelLayout layout, Transform root)
        {
            Transform group = CreateGroup("Exit", root);
            GameObject exit = CreateVisual("LevelExit", EditorAssets.LoadArt("Items/Flag"), new Vector2(layout.Length - ExitDistanceFromEnd, -0.1f), new Vector2(1.2f, 2.8f), group, PropOrder);
            exit.layer = EditorAssets.RequireLayer(GameLayers.InteractableName);
            exit.GetComponentInChildren<SpriteRenderer>().color = ExitTint;

            BoxCollider2D trigger = exit.AddComponent<BoxCollider2D>();
            trigger.size = new Vector2(2f, 5f);
            trigger.isTrigger = true;

            exit.AddComponent<LevelExit>();
        }

        private static void BuildStoryCharacters(LevelLayout layout, Transform root)
        {
            if (layout.Npcs.Count == 0)
            {
                return;
            }

            Transform group = CreateGroup("StoryCharacters", root);

            foreach (LayoutNpc npc in layout.Npcs)
            {
                ActorProfile profile = ActorCatalog.Get(npc.Actor);
                float y = LevelLayoutPlanner.GroundTop + profile.FeetOffset * npc.Scale + 0.05f + npc.HeightAboveGround;

                GameObject character = new GameObject("Story_" + npc.Speaker);
                character.transform.SetParent(group, false);
                character.transform.position = new Vector2(npc.X, y);

                GameObject visual = new GameObject("Visual");
                visual.transform.SetParent(character.transform, false);
                visual.transform.localScale = new Vector3(npc.Scale, npc.Scale, 1f);

                SpriteRenderer display = visual.AddComponent<SpriteRenderer>();
                display.sortingOrder = 8;
                display.color = npc.Tint;

                SpriteFrameAnimator animator = visual.AddComponent<SpriteFrameAnimator>();
                SpriteClip[] clips = PrefabBuilder.CreateIdleClip(npc.Actor);
                animator.SetClips(clips);
                display.sprite = clips[0].Frames[0];

                if (npc.Floating)
                {
                    FloatingBob bob = visual.AddComponent<FloatingBob>();
                    bob.Amplitude = 0.2f;
                    bob.Frequency = 1.6f;
                }

                StoryNpc story = character.AddComponent<StoryNpc>();
                story.Speaker = npc.Speaker;
                story.Line = npc.Line;
                story.VanishAfterLine = npc.VanishAfterLine;
            }
        }

        private static FollowCamera2D BuildCamera(LevelLayout layout, PiterController player)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(8f, CameraFixedY, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = CameraSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50f;

            cameraObject.AddComponent<AudioListener>();

            FollowCamera2D follow = cameraObject.AddComponent<FollowCamera2D>();
            follow.Target = player.transform;
            follow.Offset = new Vector2(3f, 0f);
            follow.FixedY = CameraFixedY;
            follow.MinX = 0f;
            follow.MaxX = layout.Length;
            follow.RightOnly = true;

            return follow;
        }

        private static void BuildBackground(LevelLayout layout, Transform cameraTransform)
        {
            GameObject root = new GameObject("Background_" + layout.Biome);
            root.transform.position = new Vector3(8f, CameraFixedY, 0f);

            for (int layer = 0; layer < ParallaxStrengths.Length; layer++)
            {
                GameObject piece = new GameObject("Parallax_" + layer);
                piece.transform.SetParent(root.transform, false);
                piece.transform.localPosition = new Vector3(0f, 0f, 5f + layer);

                SpriteRenderer display = piece.AddComponent<SpriteRenderer>();
                display.sprite = EditorAssets.LoadSprite(GameAssetPaths.BackgroundLayer(layout.Index, layout.Biome, layer));
                display.sortingOrder = -20 + layer;

                ParallaxLayer parallax = piece.AddComponent<ParallaxLayer>();
                parallax.Strength = ParallaxStrengths[layer];
                parallax.CameraTransform = cameraTransform;
                parallax.FollowCameraY = true;
            }
        }

        private static GameObject InstantiateActor(string actor, Vector2 position, Transform parent)
        {
            GameObject prefab = EditorAssets.LoadPrefab(PrefabBuilder.CharacterPath(actor));
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = actor;
            instance.transform.position = position;

            RecordOverrides(instance.transform);

            return instance;
        }

        private static void RecordOverrides(UnityEngine.Object target)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(target))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }
        }

        private static Transform CreateGroup(string name, Transform parent)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);

            return group;
        }

        private static void AddBob(GameObject pickup)
        {
            Transform visual = pickup.transform.Find("Visual");

            if (visual != null)
            {
                visual.gameObject.AddComponent<FloatingBob>();
            }
        }

        private static GameObject CreateTiled(string name, Sprite sprite, Vector2 position, Vector2 size, Transform parent)
        {
            GameObject piece = new GameObject(name);
            piece.transform.SetParent(parent, false);
            piece.transform.position = position;

            SpriteRenderer display = piece.AddComponent<SpriteRenderer>();
            display.sprite = sprite;
            display.sortingOrder = TerrainOrder;
            display.drawMode = SpriteDrawMode.Tiled;
            display.size = size;

            piece.AddComponent<BoxCollider2D>().size = size;

            return piece;
        }

        private static GameObject CreateVisual(string name, Sprite sprite, Vector2 position, Vector2 size, Transform parent, int order)
        {
            GameObject piece = new GameObject(name);
            piece.transform.SetParent(parent, false);
            piece.transform.position = position;

            GameObject art = new GameObject("Visual");
            art.transform.SetParent(piece.transform, false);

            Rect rect = GetOpaqueRect(sprite);
            int count = GetRepeatCount(rect, size);
            float cellWidth = size.x / count;
            float scale = GetUniformScale(rect, size, count);
            float bottom = -size.y * 0.5f + rect.height * scale * 0.5f;

            for (int copy = 0; copy < count; copy++)
            {
                GameObject tile = new GameObject("Sprite_" + copy);
                tile.transform.SetParent(art.transform, false);

                SpriteRenderer display = tile.AddComponent<SpriteRenderer>();
                display.sprite = sprite;
                display.sortingOrder = order;

                float x = -size.x * 0.5f + cellWidth * (copy + 0.5f);
                tile.transform.localScale = new Vector3(scale, scale, 1f);
                tile.transform.localPosition = new Vector3(x - rect.center.x * scale, bottom - rect.center.y * scale, 0f);
            }

            return piece;
        }

        private static void AddFittedCollider(GameObject piece, Sprite sprite, Vector2 size, bool trigger)
        {
            Rect rect = GetOpaqueRect(sprite);
            int count = GetRepeatCount(rect, size);
            float scale = GetUniformScale(rect, size, count);
            float width = size.x - (size.x / count - rect.width * scale);
            float height = rect.height * scale;

            BoxCollider2D box = piece.AddComponent<BoxCollider2D>();
            box.size = new Vector2(width, height);
            box.offset = new Vector2(0f, (height - size.y) * 0.5f);
            box.isTrigger = trigger;
        }

        private static int GetRepeatCount(Rect rect, Vector2 size)
        {
            float targetAspect = size.x / size.y;
            float spriteAspect = rect.width / rect.height;

            return Mathf.Max(1, Mathf.RoundToInt(targetAspect / spriteAspect));
        }

        private static float GetUniformScale(Rect rect, Vector2 size, int count)
        {
            return Mathf.Min(size.x / count / rect.width, size.y / rect.height);
        }

        private static Rect GetOpaqueRect(Sprite sprite)
        {
            if (AlphaRects.TryGetValue(sprite, out Rect cached))
            {
                return cached;
            }

            Texture2D texture = new Texture2D(2, 2);

            try
            {
                texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)));

                Color32[] pixels = texture.GetPixels32();
                int left = texture.width;
                int right = 0;
                int bottom = texture.height;
                int top = 0;

                for (int y = 0; y < texture.height; y++)
                {
                    for (int x = 0; x < texture.width; x++)
                    {
                        if (pixels[y * texture.width + x].a < AlphaThreshold)
                        {
                            continue;
                        }

                        left = Mathf.Min(left, x);
                        right = Mathf.Max(right, x + 1);
                        bottom = Mathf.Min(bottom, y);
                        top = Mathf.Max(top, y + 1);
                    }
                }

                if (right <= left || top <= bottom)
                {
                    left = 0;
                    bottom = 0;
                    right = texture.width;
                    top = texture.height;
                }

                float unit = sprite.pixelsPerUnit;
                Rect rect = new Rect((left - texture.width * 0.5f) / unit, (bottom - texture.height * 0.5f) / unit, (right - left) / unit, (top - bottom) / unit);
                AlphaRects[sprite] = rect;

                return rect;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
