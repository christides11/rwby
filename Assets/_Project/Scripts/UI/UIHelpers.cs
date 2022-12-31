using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby
{
    public static class UIHelpers
    {
        public static bool SelectDefaultSelectable(EventSystem eventSystem, LocalPlayerData player)
        {
            if (eventSystem.currentSelectedGameObject == null
                //&& player.controllerType == PlayerControllerType.GAMEPAD
                && player.rewiredPlayer.GetAxis2D(rwby.Action.UIMovement_X, rwby.Action.UIMovement_Y).sqrMagnitude > 0)
            {
                return true;
            }

            return false;
        }

        public static void TrySelectDefault(EventSystem eventSystem, GameObject selectable, LocalPlayerData player)
        {
            if (player.controllerType != PlayerControllerType.GAMEPAD) return;
            eventSystem.SetSelectedGameObject(selectable);
        }
    }
}