using System;
using DG.Tweening;
using rwby.ui;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby
{
    public class ButtonScaleTweenAnimation : TweenSelectable
    {
        public Vector3 deselectedScale;
        public Vector3 selectedScale;
        public float scaleTime = 0.15f;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            element.localScale = deselectedScale;
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            element.DOScale(selectedScale, scaleTime).SetEase(easeInType);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            element.DOScale(deselectedScale, scaleTime).SetEase(easeOutType);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            element.DOScale(selectedScale, scaleTime).SetEase(easeInType);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            element.DOScale(deselectedScale, scaleTime).SetEase(easeOutType);
        }
    }
}