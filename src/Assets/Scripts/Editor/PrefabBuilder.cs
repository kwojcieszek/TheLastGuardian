using System;
using UnityEditor;
using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Builds character, projectile and hazard prefabs from the HD sprite library.
    /// </summary>
    public static class PrefabBuilder
    {
        private const int FramesPerClip = 6;
        private const int CharacterSortingOrder = 10;
        private const int EnemySortingOrder = 9;

        private static readonly string[] States =
        {
            SpriteFrameAnimator.Idle, SpriteFrameAnimator.Run, SpriteFrameAnimator.Attack,
            SpriteFrameAnimator.Jump, SpriteFrameAnimator.Hit, SpriteFrameAnimator.Death
        };

        private static readonly float[] PiterFrameRates = { 8f, 12f, 16f, 10f, 16f, 10f };
        private static readonly float[] ActorFrameRates = { 8f, 12f, 12f, 10f, 14f, 10f };
        private static readonly Color OrbColor = new Color(0.85f, 0.45f, 1f, 1f);
        private static readonly Color FireballColor = new Color(1f, 0.55f, 0.2f, 1f);
        private static readonly Color RuneColor = new Color(1f, 0.35f, 0.15f, 1f);

        public static string CharacterPath(string actor)
        {
            return GameAssetPaths.CharacterPrefabs + actor + ".prefab";
        }

        public static string GameplayPath(string name)
        {
            return GameAssetPaths.GameplayPrefabs + name + ".prefab";
        }

        public static void BuildAll()
        {
            EditorAssets.EnsureFolder(GameAssetPaths.CharacterPrefabs);
            EditorAssets.EnsureFolder(GameAssetPaths.GameplayPrefabs);

            Projectile orb = SaveProjectile("MagicOrb", OrbColor, true, 0.6f);
            Projectile fireball = SaveProjectile("Fireball", FireballColor, false, 0.55f);
            FireRune rune = SaveFireRune();

            SavePlayer();
            SaveAleksandra();

            foreach (string enemy in new[] { ActorCatalog.Goblin, ActorCatalog.Skeleton, ActorCatalog.Wolf, ActorCatalog.Harpy, ActorCatalog.Golem })
            {
                SaveEnemy(enemy, null);
            }

            SaveEnemy(ActorCatalog.Dragon, actor =>
            {
                EnemyShooter shooter = actor.AddComponent<EnemyShooter>();
                shooter.ProjectilePrefab = fireball;
            });

            EnemyController skeleton = EditorAssets.LoadPrefab(CharacterPath(ActorCatalog.Skeleton)).GetComponent<EnemyController>();

            SaveEnemy(ActorCatalog.Malachor, actor =>
            {
                actor.GetComponent<EnemyController>().IsBoss = true;

                MalachorBoss boss = actor.AddComponent<MalachorBoss>();
                boss.OrbPrefab = orb;
                boss.MinionPrefab = skeleton;
                boss.RunePrefab = rune;
                boss.MinionSpawnY = ActorCatalog.Get(ActorCatalog.Skeleton).GroundedRootY(LevelLayoutPlanner.GroundTop);
                boss.RuneY = LevelLayoutPlanner.GroundTop + 0.15f;
            });

            AssetDatabase.SaveAssets();
        }

        public static SpriteClip[] CreateIdleClip(string actor)
        {
            SpriteClip[] all = CreateClips(actor, ActorFrameRates);

            return new[] { all[0] };
        }

        private static SpriteClip[] CreateClips(string actor, float[] frameRates)
        {
            SpriteClip[] clips = new SpriteClip[States.Length];

            for (int state = 0; state < States.Length; state++)
            {
                Sprite[] frames = new Sprite[FramesPerClip];

                for (int frame = 0; frame < FramesPerClip; frame++)
                {
                    frames[frame] = EditorAssets.LoadSprite(GameAssetPaths.AnimationFrame(actor, States[state], frame));
                }

                bool loop = state < 2;
                bool hold = States[state] == SpriteFrameAnimator.Death;
                clips[state] = new SpriteClip(States[state], frames, frameRates[state], loop, hold);
            }

            return clips;
        }

        private static GameObject CreateActorBase(string actor, string layerName, int sortingOrder, float[] frameRates)
        {
            ActorProfile profile = ActorCatalog.Get(actor);
            GameObject root = new GameObject(actor);
            root.layer = EditorAssets.RequireLayer(layerName);

            SpriteRenderer display = root.AddComponent<SpriteRenderer>();
            display.sortingOrder = sortingOrder;

            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.gravityScale = profile.IsFlying ? 0f : 2.5f;

            CapsuleCollider2D capsule = root.AddComponent<CapsuleCollider2D>();
            capsule.size = profile.ColliderSize;
            capsule.offset = new Vector2(0f, profile.ColliderOffsetY);
            capsule.direction = profile.HorizontalCapsule ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;

            SpriteFrameAnimator animator = root.AddComponent<SpriteFrameAnimator>();
            SpriteClip[] clips = CreateClips(actor, frameRates);
            animator.SetClips(clips);
            display.sprite = clips[0].Frames[0];

            return root;
        }

        private static void SavePlayer()
        {
            GameObject player = CreateActorBase(ActorCatalog.Piter, GameLayers.PlayerName, CharacterSortingOrder, PiterFrameRates);

            try
            {
                ActorProfile profile = ActorCatalog.Get(ActorCatalog.Piter);
                player.tag = "Player";

                Health health = player.AddComponent<Health>();
                health.MaxHealth = profile.MaxHealth;
                health.InvulnerabilitySeconds = 1f;
                health.DestroyOnDeath = false;

                player.AddComponent<PiterInput>();

                Transform groundCheck = new GameObject("GroundCheck").transform;
                groundCheck.SetParent(player.transform, false);
                groundCheck.localPosition = new Vector3(0f, -profile.FeetOffset, 0f);

                PiterController controller = player.AddComponent<PiterController>();
                controller.GroundCheck = groundCheck;

                DamageFlash flash = player.AddComponent<DamageFlash>();
                flash.BlinkWhileInvulnerable = true;

                PrefabUtility.SaveAsPrefabAsset(player, CharacterPath(ActorCatalog.Piter));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(player);
            }
        }

        private static void SaveAleksandra()
        {
            GameObject sister = CreateActorBase(ActorCatalog.Aleksandra, GameLayers.InteractableName, CharacterSortingOrder, ActorFrameRates);

            try
            {
                Rigidbody2D body = sister.GetComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;
                body.gravityScale = 0f;

                sister.GetComponent<CapsuleCollider2D>().isTrigger = true;

                PrefabUtility.SaveAsPrefabAsset(sister, CharacterPath(ActorCatalog.Aleksandra));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sister);
            }
        }

        private static void SaveEnemy(string actor, Action<GameObject> customize)
        {
            ActorProfile profile = ActorCatalog.Get(actor);
            GameObject enemy = CreateActorBase(actor, GameLayers.EnemyName, EnemySortingOrder, ActorFrameRates);

            try
            {
                Health health = enemy.AddComponent<Health>();
                health.MaxHealth = profile.MaxHealth;
                health.InvulnerabilitySeconds = actor == ActorCatalog.Malachor ? 0.35f : 0.25f;
                health.DestroyOnDeath = true;
                health.DestroyDelay = 0.9f;

                EnemyController controller = enemy.AddComponent<EnemyController>();
                controller.DisplayName = actor;
                controller.Movement = profile.Movement;
                controller.MoveSpeed = profile.MoveSpeed;
                controller.ChargeSpeed = profile.ChargeSpeed;
                controller.DetectionDistance = profile.DetectionDistance;
                controller.PatrolDistance = profile.PatrolDistance;
                controller.Stompable = profile.Stompable;

                enemy.AddComponent<DamageFlash>();

                CreateContactZone(enemy, profile);
                customize?.Invoke(enemy);

                PrefabUtility.SaveAsPrefabAsset(enemy, CharacterPath(actor));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(enemy);
            }
        }

        private static void CreateContactZone(GameObject enemy, ActorProfile profile)
        {
            GameObject zone = new GameObject("ContactZone");
            zone.layer = EditorAssets.RequireLayer(GameLayers.InteractableName);
            zone.transform.SetParent(enemy.transform, false);

            CapsuleCollider2D trigger = zone.AddComponent<CapsuleCollider2D>();
            trigger.isTrigger = true;
            trigger.size = profile.ColliderSize + new Vector2(0.15f, 0.1f);
            trigger.offset = new Vector2(0f, profile.ColliderOffsetY);
            trigger.direction = profile.HorizontalCapsule ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;

            zone.AddComponent<EnemyContactZone>();
        }

        private static Projectile SaveProjectile(string name, Color color, bool reflectable, float size)
        {
            GameObject root = new GameObject(name);
            root.layer = EditorAssets.RequireLayer(GameLayers.ProjectileName);

            try
            {
                Rigidbody2D body = root.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                CircleCollider2D trigger = root.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = size * 0.4f;

                Sprite crystal = EditorAssets.LoadArt("Items/Crystal");
                GameObject visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);

                float scale = size / crystal.bounds.size.y;
                visual.transform.localScale = new Vector3(scale, scale, 1f);

                SpriteRenderer display = visual.AddComponent<SpriteRenderer>();
                display.sprite = crystal;
                display.color = color;
                display.sortingOrder = 12;

                Projectile projectile = root.AddComponent<Projectile>();
                projectile.Reflectable = reflectable;

                GameObject asset = PrefabUtility.SaveAsPrefabAsset(root, GameplayPath(name));

                return asset.GetComponent<Projectile>();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static FireRune SaveFireRune()
        {
            GameObject root = new GameObject("FireRune");
            root.layer = EditorAssets.RequireLayer(GameLayers.InteractableName);

            try
            {
                BoxCollider2D trigger = root.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(1.6f, 0.6f);
                trigger.offset = new Vector2(0f, 0.2f);

                Sprite lava = EditorAssets.LoadArt("Tiles/Lava_64");
                GameObject visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);
                visual.transform.localScale = new Vector3(1.6f / lava.bounds.size.x, 0.3f / lava.bounds.size.y, 1f);

                SpriteRenderer display = visual.AddComponent<SpriteRenderer>();
                display.sprite = lava;
                display.color = RuneColor;
                display.sortingOrder = 6;

                root.AddComponent<FireRune>();

                GameObject asset = PrefabUtility.SaveAsPrefabAsset(root, GameplayPath("FireRune"));

                return asset.GetComponent<FireRune>();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
