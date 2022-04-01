using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace rwby.ui
{
    public class Selectable : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, ISubmitHandler, IPointerClickHandler
    {
        public bool active = true;
        public UnityEvent onSelect;
        public UnityEvent onDeselect;
        public UnityEvent onSubmit;

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
    }
}