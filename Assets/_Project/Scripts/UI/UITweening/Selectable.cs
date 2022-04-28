using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby.ui
{
    public class Selectable : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, ISubmitHandler, IPointerClickHandler, IMoveHandler
    {
        public bool active = true;
        public UnityEvent onSelect;
        public UnityEvent onDeselect;
        public UnityEvent onSubmit;

        public Selectable selectOnUp;
        public Selectable selectOnDown;
        public Selectable selectOnLeft;
        public Selectable selectOnRight;
        
        public virtual void Awake()
        {
            
        }

        public virtual void Start()
        {
            
        }

        public virtual void OnEnable()
        {
            
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (!active) return;
            onSelect.Invoke();
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (!active) return;
            onDeselect.Invoke();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!active) return;
            onSelect.Invoke();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!active) return;
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
        
        public virtual void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                    Navigate(eventData, selectOnRight);
                    break;

                case MoveDirection.Up:
                    Navigate(eventData, selectOnUp);
                    break;

                case MoveDirection.Left:
                    Navigate(eventData, selectOnLeft);
                    break;

                case MoveDirection.Down:
                    Navigate(eventData, selectOnDown);
                    break;
            }
        }
        
        public virtual void Navigate(AxisEventData eventData, Selectable sel)
        {
            if (sel != null && sel.active)
                eventData.selectedObject = sel.gameObject;
        }
    }
}