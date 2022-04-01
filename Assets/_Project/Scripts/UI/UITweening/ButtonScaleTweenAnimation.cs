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
        
        public override void OnEnable()
        {
            element.localScale = deselectedScale;
        }

        public override void OnSelect(BaseEventData eventData)
        {
            element.DOScale(selectedScale, scaleTime).SetEase(easeInType);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            element.DOScale(deselectedScale, scaleTime).SetEase(easeOutType);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            element.DOScale(selectedScale, scaleTime).SetEase(easeInType);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            element.DOScale(deselectedScale, scaleTime).SetEase(easeOutType);
        }
    }
}