using System.Collections.Generic;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Kinematic body that moves between two points and carries the player standing on it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class MovingPlatform2D : MonoBehaviour
    {
        private const float RiderTolerance = 0.2f;

        private readonly HashSet<Rigidbody2D> riders = new HashSet<Rigidbody2D>();
        private Rigidbody2D body;
        private BoxCollider2D surface;
        private float elapsed;

        [field: SerializeField] public Vector2 PointA { get; set; }

        [field: SerializeField] public Vector2 PointB { get; set; }

        [field: SerializeField] public float Speed { get; set; } = 1.8f;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            surface = GetComponent<BoxCollider2D>();
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;

            float distance = Vector2.Distance(PointA, PointB);

            if (distance < 0.01f)
            {
                return;
            }

            float travelled = Mathf.PingPong(elapsed * Speed, distance) / distance;
            Vector2 destination = Vector2.Lerp(PointA, PointB, travelled);
            Vector2 delta = destination - body.position;

            foreach (Rigidbody2D rider in riders)
            {
                if (rider != null && rider.position.y > body.position.y && rider.linearVelocity.y <= 0.1f)
                {
                    rider.position += delta;
                }
            }

            body.MovePosition(destination);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            Rigidbody2D rider = collision.rigidbody;

            if (rider == null || rider.GetComponent<PiterController>() == null)
            {
                return;
            }

            if (collision.collider.bounds.min.y >= surface.bounds.max.y - RiderTolerance)
            {
                riders.Add(rider);
            }
            else
            {
                riders.Remove(rider);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.rigidbody != null)
            {
                riders.Remove(collision.rigidbody);
            }
        }
    }
}
