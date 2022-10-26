using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby.ui
{
    public class TweenSelectable : Selectable
    {
        public Transform element;

        [SelectImplementation(typeof(BaseTween))] [SerializeField, SerializeReference]
        public BaseTween[] tweens = Array.Empty<BaseTween>();
        
        protected override void OnEnable()
        {
            base.OnEnable();
            foreach (BaseTween bt in tweens)
            {
                bt.DOTweenOff(element);
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            foreach (BaseTween bt in tweens)
            {
                bt.DOTweenOn(element);
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            foreach (BaseTween bt in tweens)
            {
                bt.DOTweenOff(element);
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            foreach (BaseTween bt in tweens)
            {
                bt.DOTweenOn(element);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            foreach (BaseTween bt in tweens)
            {
                bt.DOTweenOff(element);
            }
        }
    }
}