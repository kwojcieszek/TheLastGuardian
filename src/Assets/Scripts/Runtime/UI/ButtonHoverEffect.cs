using UnityEngine;
using UnityEngine.EventSystems;

namespace PiterGame
{
    /// <summary>
    /// Smoothly enlarges a button while it is hovered or selected.
    /// </summary>
    public sealed class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private const float HoverScale = 1.06f;
        private const float Sharpness = 14f;

        private bool hovered;
        private bool selected;

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
        }

        private void OnDisable()
        {
            hovered = false;
            selected = false;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            float target = hovered || selected ? HoverScale : 1f;
            float current = transform.localScale.x;
            float next = Mathf.Lerp(current, target, 1f - Mathf.Exp(-Sharpness * Time.unscaledDeltaTime));

            transform.localScale = new Vector3(next, next, 1f);
        }
    }
}
