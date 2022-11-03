using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby.ui
{
    public class ContentButtonInputField : ContentButtonBase
    {
        public TMP_InputField inputField;
        public LayoutElement LayoutElement;
        
        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
        }
    }
}