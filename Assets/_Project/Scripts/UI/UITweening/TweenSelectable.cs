using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby.ui
{
    public class TweenSelectable : Selectable
    {
        public Transform element;
        
        public Ease easeInType = Ease.OutCirc;
        public Ease easeOutType = Ease.InCirc;
    }
}