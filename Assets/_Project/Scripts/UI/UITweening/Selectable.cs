using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace rwby.ui
{
    public class Selectable : UnityEngine.UI.Selectable, ISubmitHandler, IPointerClickHandler
    {
        public UnityEvent onSelect;
        public UnityEvent onDeselect;
        public UnityEvent onSubmit;

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            if (!interactable) return;
            onSelect.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            if (!interactable) return;
            onDeselect.Invoke();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (!interactable) return;
            onSelect.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (!interactable) return;
            onDeselect.Invoke();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (!interactable) return;
            onSubmit.Invoke();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;
            onSubmit.Invoke();
        }
    }
}