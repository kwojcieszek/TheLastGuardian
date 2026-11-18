using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Side-scrolling camera that only advances to the right and follows high jumps.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera2D : MonoBehaviour
    {
        private const float HighJumpThreshold = 1.2f;

        private Camera viewCamera;
        private Vector3 velocity;
        private float furthestX = float.MinValue;

        [field: SerializeField] public Transform Target { get; set; }

        [field: SerializeField] public Vector2 Offset { get; set; } = new Vector2(3f, 0f);

        [field: SerializeField] public float Smoothing { get; set; } = 0.12f;

        [field: SerializeField] public bool RightOnly { get; set; } = true;

        [field: SerializeField] public float MinX { get; set; }

        [field: SerializeField] public float MaxX { get; set; } = 1000f;

        [field: SerializeField] public float FixedY { get; set; } = 1f;

        public void SnapToTarget()
        {
            if (Target == null)
            {
                return;
            }

            furthestX = float.MinValue;
            velocity = Vector3.zero;
            transform.position = new Vector3(GoalX(), GoalY(), transform.position.z);
        }

        private void Awake()
        {
            viewCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            Vector3 goal = new Vector3(GoalX(), GoalY(), transform.position.z);
            Vector3 position = Vector3.SmoothDamp(transform.position, goal, ref velocity, Smoothing);

            if (RightOnly)
            {
                position.x = Mathf.Max(transform.position.x, position.x);
            }

            transform.position = position;
        }

        private float GoalX()
        {
            if (viewCamera == null)
            {
                viewCamera = GetComponent<Camera>();
            }

            float halfWidth = viewCamera.orthographicSize * viewCamera.aspect;
            float left = MinX + halfWidth;
            float right = MaxX - halfWidth;
            float x = right < left ? (MinX + MaxX) * 0.5f : Mathf.Clamp(Target.position.x + Offset.x, left, right);

            if (RightOnly)
            {
                x = Mathf.Max(x, furthestX);
            }

            furthestX = x;

            return x;
        }

        private float GoalY()
        {
            float lift = Mathf.Max(0f, Target.position.y - FixedY - HighJumpThreshold);

            return FixedY + Offset.y + lift;
        }
    }
}
