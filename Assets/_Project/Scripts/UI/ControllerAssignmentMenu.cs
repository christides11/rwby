using Rewired;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace rwby
{
    public class ControllerAssignmentMenu : MonoBehaviour
    {
        public LocalPlayerManager localPlayerManager;
        
        public ReorderableList globalControllerGrid;
        public ReorderableList p1ControllerGrid;
        public ReorderableList p2ControllerGrid;
        public ReorderableList p3ControllerGrid;
        public ReorderableList p4ControllerGrid;

        public ControllerAssignmentElement controllerPrefab;
        
        public void OpenMenu()
        {
            ClearChildren();
            ResetAllControllers();
            
            var joysticks = ReInput.controllers.GetControllers(ControllerType.Joystick);
            for (int i = 0; i < joysticks.Length; i++)
            {
                var c = GameObject.Instantiate(controllerPrefab, globalControllerGrid.ContentLayout.transform, false);
                c.controllerType = ControllerType.Joystick;
                c.controllerID = joysticks[i].id;
            }
        }

        private void ResetAllControllers()
        {
            foreach (var p in localPlayerManager.localPlayers)
            {
                p.rewiredPlayer.controllers.ClearAllControllers();
            }
        }

        private void ClearChildren()
        {
            foreach (Transform child in globalControllerGrid.ContentLayout.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in p1ControllerGrid.ContentLayout.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in p2ControllerGrid.ContentLayout.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}