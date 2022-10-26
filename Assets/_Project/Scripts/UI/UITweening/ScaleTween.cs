using DG.Tweening;
using UnityEngine;

namespace rwby.ui
{
    [System.Serializable]
    public class ScaleTween : BaseTween
    {
        public Vector3 selectedScale = new Vector3(1, 1, 1);
        public Vector3 deselectedScale = new Vector3(1, 1, 1);
        
        public override void DOTweenOn(Transform element)
        {
            base.DOTweenOn(element);
            element.DOScale(selectedScale, scaleTime).SetEase(easeInType);
        }

        public override void DOTweenOff(Transform element)
        {
            base.DOTweenOff(element);
            element.DOScale(deselectedScale, scaleTime).SetEase(easeOutType);
        }
    }
}