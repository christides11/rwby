using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby.ui
{
    public class SelectableButton : rwby.ui.Selectable
    {
        public Image target;
        public Color normalColor;
        public Color selectedColor;
        public Color submitColor;

        protected override void OnEnable()
        {
            base.OnEnable();
            target.color = normalColor;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            target.color = selectedColor;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            target.color = normalColor;
        }
    }
}