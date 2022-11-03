using rwby.ui;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby
{
    public class LobbySettingsIntValueContent : ContentButtonBase
    {
        public rwby.ui.Selectable selectableSubtract;
        public rwby.ui.Selectable selectableAdd;
        public LayoutElement LayoutElement;

        public override void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Down:
                case MoveDirection.Up:
                    base.OnMove(eventData);
                    break;
                case MoveDirection.Left:
                    selectableSubtract.onSubmit.Invoke();
                    break;
                case MoveDirection.Right:
                    selectableAdd.onSubmit.Invoke();
                    break;
            }
        }
    }
}