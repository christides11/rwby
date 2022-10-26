using DG.Tweening;
using UnityEngine;

namespace rwby.ui
{
    [System.Serializable]
    public class BaseTween
    {
        public Ease easeInType = Ease.OutCirc;
        public Ease easeOutType = Ease.InCirc;
        public float scaleTime = 0.05f;
        
        public virtual void DOTweenOn(Transform element)
        {
            
        }

        public virtual void DOTweenOff(Transform element)
        {
            
        }
    }
}