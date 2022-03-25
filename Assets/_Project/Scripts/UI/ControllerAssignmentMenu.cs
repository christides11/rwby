using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace rwby
{
    public class ControllerAssignmentMenu : MonoBehaviour
    {
        public delegate void MenuAction(ControllerAssignmentMenu menu);
        public event MenuAction OnControllersAssigned;

        private Dictionary<Joystick, ControllerAssignmentElement> joystickElements =
            new Dictionary<Joystick, ControllerAssignmentElement>();

        public LocalPlayerManager localPlayerManager;
        
        public ReorderableList globalControllerGrid;
        public ReorderableList[] playerControllerGrids;

        public ControllerAssignmentElement controllerPrefab;

        private Rewired.Player systemPlayer;

        public void OpenMenu(int playerCount = -1)
        {
            systemPlayer = ReInput.players.GetSystemPlayer();
            localPlayerManager.CollectJoysticks();
            Cleanup();
            ResetGrids();
            gameObject.SetActive(true);
        }

        public void CloseMenu()
        {
            Cleanup();
            gameObject.SetActive(false);
        }

        public bool AssignControllers()
        {
            List<ReorderableList> validPlayers = new List<ReorderableList>();
            foreach (var g in playerControllerGrids)
            {
                if (g.ContentLayout.transform.childCount == 0) continue;
                validPlayers.Add(g);
            }
            if (validPlayers.Count < 1) return false;
            localPlayerManager.SetPlayerCount(validPlayers.Count);

            for (int i = 0; i < validPlayers.Count; i++)
            {
                foreach (Transform cae in validPlayers[i].ContentLayout.transform)
                {
                    localPlayerManager.GiveController(i, cae.GetComponent<ControllerAssignmentElement>().joystick);
                }
            }
            
            OnControllersAssigned?.Invoke(this);
            return true;
        }

        private void Update()
        {
            if (systemPlayer.GetButtonDown(rwby.Action.Confirm))
            {
                AssignControllers();
            }
        }

        public float assignmentDeadzone = 0.2f;
        private void FixedUpdate()
        {
            Vector2 prev = Vector2.zero;
            Vector2 axis = Vector2.zero;
            foreach (var c in systemPlayer.controllers.Joysticks)
            {
                if (c.axis2DCount == 0) continue;
                prev = c.GetAxis2DPrev(0);
                axis = c.GetAxis2D(0);
                
                
                if (Mathf.Abs(axis.x) < assignmentDeadzone)
                {
                    if (Mathf.Abs(prev.y) > assignmentDeadzone) continue;
                    if (axis.y > assignmentDeadzone)
                    {
                        joystickElements[c].transform.SetParent(playerControllerGrids[0].ContentLayout.transform);
                    }
                    else if(axis.y < -assignmentDeadzone)
                    {
                        joystickElements[c].transform.SetParent(playerControllerGrids[2].ContentLayout.transform);
                    }
                }
                else
                {
                    if (Mathf.Abs(prev.x) > assignmentDeadzone) continue;
                    if (axis.x > assignmentDeadzone)
                    {
                        joystickElements[c].transform.SetParent(playerControllerGrids[1].ContentLayout.transform);
                    }
                    else if(axis.x < -assignmentDeadzone)
                    {
                        joystickElements[c].transform.SetParent(playerControllerGrids[3].ContentLayout.transform);
                    }
                }
            }
        }
        
        void FillControllerGrid(ReorderableList grid, IList<Joystick> controllers)
        {
            for (int i = 0; i < controllers.Count; i++)
            {
                var c = GameObject.Instantiate(controllerPrefab, grid.ContentLayout.transform, false);
                c.joystick = controllers[i];
                joystickElements.Add(controllers[i], c);
            }
        }

        private void Cleanup()
        {
            ClearChildren();
            joystickElements.Clear();
        }

        void ResetGrids()
        {
            var systemJoysticks = systemPlayer.controllers.Joysticks;
            FillControllerGrid(globalControllerGrid, systemJoysticks);

            for (int i = 0; i < localPlayerManager.localPlayers.Count; i++)
            {
                var pJoysticks = localPlayerManager.localPlayers[0].rewiredPlayer.controllers.Joysticks;
                FillControllerGrid(playerControllerGrids[i], pJoysticks);
            }
        }
        
        private void ClearChildren()
        {
            foreach (Transform child in globalControllerGrid.ContentLayout.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var pGrid in playerControllerGrids)
            {
                foreach (Transform child in pGrid.ContentLayout.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}