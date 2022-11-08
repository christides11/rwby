using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby.ui
{
    public class ButtonFloatSlider : rwby.ui.Selectable
    {
        public TextMeshProUGUI text;
        public TextMeshProUGUI valueText;

        public Slider slider;

        public float stepSize = 0.1f;

        protected override void Awake()
        {
            base.Awake();
            slider.onValueChanged.AddListener(UpdateValue);
        }

        private void UpdateValue(float arg0)
        {
            valueText.text = arg0.ToString();
        }

        public override void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                    slider.value += stepSize;
                    break;
                case MoveDirection.Up:
                    base.OnMove(eventData);
                    break;
                case MoveDirection.Left:
                    slider.value -= stepSize;
                    break;
                case MoveDirection.Down:
                    base.OnMove(eventData);
                    break;
            }
        }
    }
}