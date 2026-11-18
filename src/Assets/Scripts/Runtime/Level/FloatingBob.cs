using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Gentle vertical bobbing for pickups.
    /// </summary>
    public sealed class FloatingBob : MonoBehaviour
    {
        private Vector3 basePosition;
        private float phase;

        [field: SerializeField] public float Amplitude { get; set; } = 0.1f;

        [field: SerializeField] public float Frequency { get; set; } = 2.5f;

        private void Start()
        {
            basePosition = transform.localPosition;
            phase = transform.position.x * 0.7f;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * Frequency + phase) * Amplitude;
            transform.localPosition = basePosition + new Vector3(0f, offset, 0f);
        }
    }
}
