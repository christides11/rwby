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
            onSelect.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            onDeselect.Invoke();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            onSelect.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            onDeselect.Invoke();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            onSubmit.Invoke();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            onSubmit.Invoke();
        }
    }
}