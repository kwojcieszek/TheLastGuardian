using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Story character placed in a level. Speaks a line when Piter approaches and can vanish afterwards.
    /// </summary>
    public sealed class StoryNpc : MonoBehaviour
    {
        private const float VanishDelay = 1.6f;
        private const float FadeSeconds = 1.1f;
        private const float VanishRise = 1.2f;

        private SpriteRenderer display;
        private Color baseColor;
        private Vector3 basePosition;
        private float fadeStart;
        private bool triggered;

        [field: SerializeField] public string Speaker { get; set; }

        [field: SerializeField] public string Line { get; set; }

        [field: SerializeField] public float TriggerDistance { get; set; } = 6f;

        [field: SerializeField] public bool VanishAfterLine { get; set; }

        [field: SerializeField] public bool FacePlayer { get; set; } = true;

        public bool HasSpoken => triggered;

        private void Awake()
        {
            display = GetComponentInChildren<SpriteRenderer>();
            baseColor = display.color;
        }

        private void Update()
        {
            LevelSession session = LevelSession.Current;

            if (session == null || session.Player == null)
            {
                return;
            }

            Vector3 playerPosition = session.Player.transform.position;

            if (FacePlayer)
            {
                display.flipX = playerPosition.x < transform.position.x;
            }

            if (!triggered)
            {
                TryTrigger(session, playerPosition);

                return;
            }

            if (VanishAfterLine)
            {
                UpdateVanish();
            }
        }

        private void TryTrigger(LevelSession session, Vector3 playerPosition)
        {
            if (session.State != LevelState.Playing || Mathf.Abs(playerPosition.x - transform.position.x) > TriggerDistance)
            {
                return;
            }

            triggered = true;
            fadeStart = Time.time + VanishDelay;
            basePosition = transform.position;

            session.ShowToast($"{Speaker}: \"{Line}\"");
            GameAudio.Play(SoundEffect.Objective, 0.6f);
        }

        private void UpdateVanish()
        {
            float progress = (Time.time - fadeStart) / FadeSeconds;

            if (progress <= 0f)
            {
                return;
            }

            if (progress >= 1f)
            {
                gameObject.SetActive(false);

                return;
            }

            Color color = baseColor;
            color.a = baseColor.a * (1f - progress);
            display.color = Color.Lerp(color, new Color(0.75f, 0.4f, 1f, color.a), progress);
            transform.position = basePosition + new Vector3(0f, VanishRise * progress, 0f);
        }
    }
}
