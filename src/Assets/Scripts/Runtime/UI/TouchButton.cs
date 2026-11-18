using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Hold-style on-screen button that feeds the shared touch input state.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Image image;
        private bool pressed;

        [field: SerializeField] public TouchButtonAction Action { get; set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetPressed(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetPressed(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetPressed(false);
        }

        private void Awake()
        {
            image = GetComponent<Image>();
            image.color = UiTheme.TouchButtonColor;
        }

        private void OnDisable()
        {
            SetPressed(false);
        }

        private void SetPressed(bool value)
        {
            if (pressed == value)
            {
                return;
            }

            pressed = value;
            TouchInputState.SetButton(Action, value);

            if (image != null)
            {
                image.color = value ? UiTheme.TouchButtonPressed : UiTheme.TouchButtonColor;
            }
        }
    }
}
