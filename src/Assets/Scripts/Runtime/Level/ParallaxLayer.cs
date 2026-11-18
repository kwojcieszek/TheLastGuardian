using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Background layer that scrolls slower than the camera and repeats endlessly.
    /// </summary>
    public sealed class ParallaxLayer : MonoBehaviour
    {
        private float startX;
        private float startY;
        private float cameraStartX;
        private float cameraStartY;
        private float repeatWidth;
        private bool originalFlip;
        private SpriteRenderer mainDisplay;
        private SpriteRenderer[] repeats;

        [field: SerializeField] public Transform CameraTransform { get; set; }

        [field: SerializeField, Range(0f, 1f)] public float Strength { get; set; } = 0.3f;

        [field: SerializeField] public bool RepeatHorizontally { get; set; } = true;

        [field: SerializeField] public bool MirrorAlternating { get; set; } = true;

        [field: SerializeField] public bool FollowCameraY { get; set; } = true;

        private void Start()
        {
            if (CameraTransform == null && Camera.main != null)
            {
                CameraTransform = Camera.main.transform;
            }

            startX = transform.position.x;
            startY = transform.position.y;

            if (CameraTransform != null)
            {
                cameraStartX = CameraTransform.position.x;
                cameraStartY = CameraTransform.position.y;
            }

            SpriteRenderer display = GetComponent<SpriteRenderer>();

            if (!RepeatHorizontally || display == null || display.sprite == null)
            {
                return;
            }

            CreateRepeats(display);
        }

        private void LateUpdate()
        {
            if (CameraTransform == null)
            {
                return;
            }

            Vector3 position = transform.position;
            position.x = startX + (CameraTransform.position.x - cameraStartX) * Strength;

            if (FollowCameraY)
            {
                position.y = startY + CameraTransform.position.y - cameraStartY;
            }

            if (repeatWidth > 0f)
            {
                int tileIndex = Mathf.RoundToInt((CameraTransform.position.x - position.x) / repeatWidth);
                position.x += tileIndex * repeatWidth;

                mainDisplay.flipX = originalFlip ^ (MirrorAlternating && tileIndex % 2 != 0);

                foreach (SpriteRenderer repeat in repeats)
                {
                    repeat.flipX = MirrorAlternating ? !mainDisplay.flipX : mainDisplay.flipX;
                }
            }

            transform.position = position;
        }

        private void CreateRepeats(SpriteRenderer display)
        {
            mainDisplay = display;
            originalFlip = display.flipX;
            repeats = new SpriteRenderer[2];
            repeatWidth = display.bounds.size.x;

            for (int side = 0; side < 2; side++)
            {
                int direction = side == 0 ? -1 : 1;
                GameObject tile = new GameObject("Repeat_" + direction);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(direction * display.sprite.bounds.size.x, 0f, 0f);

                SpriteRenderer copy = tile.AddComponent<SpriteRenderer>();
                copy.sprite = display.sprite;
                copy.color = display.color;
                copy.sharedMaterial = display.sharedMaterial;
                copy.sortingLayerID = display.sortingLayerID;
                copy.sortingOrder = display.sortingOrder;
                copy.flipX = MirrorAlternating ? !display.flipX : display.flipX;

                repeats[side] = copy;
            }
        }
    }
}
