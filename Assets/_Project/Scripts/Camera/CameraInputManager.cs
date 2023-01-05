using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Rewired;
using UnityEngine;

namespace rwby
{
    public class CameraInputManager : MonoBehaviour
    {
        private ClientManager clientManager;
        private int playerID;
        private Rewired.Player p;
        [SerializeField] private ProfileDefinition currentProfile;
        private PlayerControllerType currentControllerType;

        private void Awake()
        {
            GameManager.singleton.profilesManager.onProfileUpdated += (pm, index) =>
            {
                SetProfile(pm.Profiles[index]);
            };
        }

        public void AssignControlTo(ClientManager clientManager, int playerID)
        {
            this.clientManager = clientManager;
            this.playerID = playerID;
            p = ReInput.players.GetPlayer(playerID);
            SetProfile(GameManager.singleton.profilesManager.GetProfile(clientManager.profiles[playerID]));
            OnControllerTypeChanged(playerID, GameManager.singleton.localPlayerManager.GetPlayerControllerType(playerID));
            GameManager.singleton.localPlayerManager.OnPlayerControllerTypeChanged -= OnControllerTypeChanged;
            GameManager.singleton.localPlayerManager.OnPlayerControllerTypeChanged += OnControllerTypeChanged;
        }
        
        public ProfileDefinition.CameraVariables GetCameraControls()
        {
            return currentControllerType == PlayerControllerType.GAMEPAD
                ? currentProfile.controllerCam
                : currentProfile.keyboardCam;
        }

        public Vector2 GetCameraInput(bool lockOn = false)
        {
            ProfileDefinition.CameraVariables cv = GetCameraControls();
            
            Vector2 stickInput = p.GetAxis2D(Action.Camera_X, Action.Camera_Y);

            if (Mathf.Abs(stickInput.x) < cv.deadzoneHoz) stickInput.x = 0;
            if (Mathf.Abs(stickInput.y) < cv.deadzoneVert) stickInput.y = 0;
            stickInput.x *= lockOn ? cv.speedLockOnHoz : cv.speedHoz;
            stickInput.y *= lockOn ? cv.speedLockOnVert : cv.speedVert;
            
            return stickInput;
        }

        private void SetProfile(ProfileDefinition profile)
        {
            currentProfile = profile;
            GameManager.singleton.profilesManager.ApplyProfileToPlayer(playerID, profile.profileName);
        }
        
        private void OnControllerTypeChanged(int playerid, PlayerControllerType controllertype)
        {
            if (playerid != playerID) return;
            currentControllerType = controllertype;
        }
    }
}