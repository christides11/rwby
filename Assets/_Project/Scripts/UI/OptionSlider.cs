using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby.ui
{
    public class OptionSlider : rwby.ui.Selectable
    {
        public TextMeshProUGUI text;
        public RectTransform filledBar;

        public string[] options;

        public int currentOption;

        public override void OnSubmit(BaseEventData eventData)
        {
            SetOption(currentOption == (options.Length-1) ? 0 : currentOption + 1);
            base.OnSubmit(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            SetOption(currentOption == (options.Length-1) ? 0 : currentOption + 1);
            base.OnPointerClick(eventData);
        }

        public override void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                    if (currentOption == options.Length-1)
                    {
                        Navigate(eventData, selectOnLeft);
                    }
                    else
                    {
                        SetOption(currentOption+1);
                    }
                    break;
                case MoveDirection.Up:
                    Navigate(eventData, selectOnUp);
                    break;
                case MoveDirection.Left:
                    if (currentOption == 0)
                    {
                        Navigate(eventData, selectOnLeft);
                    }
                    else
                    {
                        SetOption(currentOption-1);
                    }
                    break;
                case MoveDirection.Down:
                    Navigate(eventData, selectOnDown);
                    break;
            }
        }

        public virtual void SetOption(int value)
        {
            currentOption = value;
            text.text = options[currentOption];

            Vector2 temp = filledBar.sizeDelta;
            temp.x = GetComponent<RectTransform>().sizeDelta.x / (float)options.Length;
            filledBar.sizeDelta = temp;
            Vector2 aPos = filledBar.anchoredPosition;
            aPos.x = temp.x * currentOption;
            filledBar.anchoredPosition = aPos;
        }
    }
}