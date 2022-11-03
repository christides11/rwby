using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby.ui
{
    public class ContentButtonIntValue : ContentButtonBase
    {
        public rwby.ui.Selectable subtractButton;
        public rwby.ui.Selectable addButton;
        public TextMeshProUGUI intValueText;
        public LayoutElement LayoutElement;
        
        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
        }
    }
}