using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Balancing data of all characters, based on the gameplay design document.
    /// </summary>
    public static class ActorCatalog
    {
        public const string Piter = "Piter";
        public const string Aleksandra = "Aleksandra";
        public const string Malachor = "Malachor";
        public const string Goblin = "Goblin";
        public const string Skeleton = "Skeleton";
        public const string Wolf = "Wolf";
        public const string Harpy = "Harpy";
        public const string Golem = "Golem";
        public const string Dragon = "Dragon";

        private static readonly Dictionary<string, ActorProfile> Profiles = CreateProfiles();

        public static string[] Enemies { get; } = new[] { Goblin, Skeleton, Wolf, Harpy, Golem, Dragon, Malachor };

        public static string[] AllActors { get; } = new[] { Piter, Aleksandra, Malachor, Goblin, Skeleton, Wolf, Harpy, Golem, Dragon };

        public static ActorProfile Get(string name)
        {
            if (!Profiles.TryGetValue(name, out ActorProfile profile))
            {
                throw new ArgumentException("Unknown actor: " + name);
            }

            return profile;
        }

        public static string ActorForPath(string assetPath)
        {
            foreach (string actor in AllActors)
            {
                bool character = assetPath == GameAssetPaths.ArtRoot + "Characters/" + actor + ".png";
                bool animation = assetPath.StartsWith(GameAssetPaths.ArtRoot + "Animations/" + actor + "/", StringComparison.Ordinal);

                if (character || animation)
                {
                    return actor;
                }
            }

            return null;
        }

        private static Dictionary<string, ActorProfile> CreateProfiles()
        {
            ActorProfile[] profiles =
            {
                new ActorProfile { Name = Piter, PixelsPerUnit = 240f, ColliderSize = new Vector2(0.7f, 1.5f), MaxHealth = 3 },
                new ActorProfile { Name = Aleksandra, PixelsPerUnit = 240f, ColliderSize = new Vector2(0.6f, 1.45f), MaxHealth = 3 },
                new ActorProfile
                {
                    Name = Malachor, PixelsPerUnit = 200f, ColliderSize = new Vector2(0.8f, 1.8f), MaxHealth = 25,
                    MoveSpeed = 1.4f, DetectionDistance = 9f, PatrolDistance = 5f, Stompable = false
                },
                new ActorProfile { Name = Goblin, PixelsPerUnit = 260f, ColliderSize = new Vector2(0.65f, 1.3f), MaxHealth = 2, MoveSpeed = 2f },
                new ActorProfile { Name = Skeleton, PixelsPerUnit = 240f, ColliderSize = new Vector2(0.7f, 1.5f), MaxHealth = 3, MoveSpeed = 1.8f },
                new ActorProfile
                {
                    Name = Wolf, PixelsPerUnit = 240f, ColliderSize = new Vector2(1.2f, 0.7f), HorizontalCapsule = true, MaxHealth = 2,
                    Movement = EnemyMovement.Charger, MoveSpeed = 2.4f, ChargeSpeed = 6.5f, DetectionDistance = 7f
                },
                new ActorProfile
                {
                    Name = Harpy, PixelsPerUnit = 200f, ColliderSize = new Vector2(0.8f, 1.25f), MaxHealth = 2,
                    Movement = EnemyMovement.Diver, MoveSpeed = 2.4f, ChargeSpeed = 6f, DetectionDistance = 7f
                },
                new ActorProfile { Name = Golem, PixelsPerUnit = 160f, ColliderSize = new Vector2(1.6f, 2.2f), MaxHealth = 4, MoveSpeed = 1f },
                new ActorProfile
                {
                    Name = Dragon, PixelsPerUnit = 110f, ColliderSize = new Vector2(2.7f, 1.5f), HorizontalCapsule = true, MaxHealth = 4,
                    Movement = EnemyMovement.Flyer, MoveSpeed = 1.6f, DetectionDistance = 9f
                }
            };

            Dictionary<string, ActorProfile> result = new Dictionary<string, ActorProfile>();

            foreach (ActorProfile profile in profiles)
            {
                result[profile.Name] = profile;
            }

            return result;
        }
    }
}
