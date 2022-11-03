using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby.ui
{
    public class ContentButtonStringValue : ContentButtonBase
    {
        public TextMeshProUGUI valueString;
        public LayoutElement LayoutElement;
        
        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
        }
    }
}