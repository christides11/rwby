using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace rwby
{
    public class LocalPlayerManager : MonoBehaviour
    {
        public delegate void PlayerCountAction(LocalPlayerManager localPlayerManager, int previousPlayerCount, int currentPlayCount);
        public event PlayerCountAction OnPlayerCountChanged;

        public delegate void PlayerControllerAction(int playerID, PlayerControllerType controllerType);
        public event PlayerControllerAction OnPlayerControllerTypeChanged;
        
        public int maxLocalPlayers = 4;
        public LocalPlayerData systemPlayer;
        public List<LocalPlayerData> localPlayers = new List<LocalPlayerData>();

        public CameraLayoutDefinition[] playerCameraLayouts = new CameraLayoutDefinition[4];

        public void Initialize()
        {
            systemPlayer = new LocalPlayerData() { rewiredPlayer = ReInput.players.GetSystemPlayer(), controllerType = PlayerControllerType.NONE, camera = Camera.main };
            systemPlayer.rewiredPlayer.controllers.AddLastActiveControllerChangedDelegate(WhenSystemPlayerActiveControllerChanged);
            CollectJoysticks();
            AddPlayer();
        }

        public void AutoAssignControllers()
        {
            if (localPlayers.Count < 1) return;
            
            foreach (var controller in ReInput.controllers.GetControllers(ControllerType.Joystick))
            {
                localPlayers[0].rewiredPlayer.controllers.AddController(controller, true);
            }
            
            GiveMouseKeyboard(0);
        }

        public LocalPlayerData GetPlayer(int playerID)
        {
            if (playerID == -1) return systemPlayer;
            if (playerID < 0 || playerID >= localPlayers.Count) return default;
            return localPlayers[playerID];
        }

        public void SetPlayerHUD(int playerID, BaseHUD baseHUD)
        {
            var temp = localPlayers[playerID];
            temp.hud = baseHUD;
            localPlayers[playerID] = temp;
        }
        
        public void SetPlayerCameraHandler(int playerID, CameraSwitcher handler)
        {
            var temp = localPlayers[playerID];
            temp.cameraHandler = handler;
            localPlayers[playerID] = temp;
        }
        
        public void SetPlayerCamera(int playerID, Camera camera)
        {
            var temp = localPlayers[playerID];
            temp.camera = camera;
            localPlayers[playerID] = temp;
            
            ApplyCameraLayout();
        }

        public void ApplyCameraLayout()
        {
            CameraLayoutDefinition layout = playerCameraLayouts[localPlayers.Count-1];

            for (int i = 0; i < localPlayers.Count; i++)
            {
                if (localPlayers[i].camera == null) continue;
                localPlayers[i].camera.rect = layout.cameraLayouts[i];
            }
        }
        
        public bool AddPlayer()
        {
            if (localPlayers.Count == maxLocalPlayers) return false;

            var rewiredPlayer = ReInput.players.GetPlayer(localPlayers.Count);
            
            localPlayers.Add(new LocalPlayerData()
            {
                isValid = true,
                controllerType = PlayerControllerType.NONE,
                rewiredPlayer = rewiredPlayer
            });

            rewiredPlayer.isPlaying = true;
            rewiredPlayer.controllers.AddLastActiveControllerChangedDelegate(OnPlayerActiveControllerChanged);
            OnPlayerCountChanged?.Invoke(this, localPlayers.Count-1, localPlayers.Count);
            return true;
        }

        public bool RemovePlayer(int playerID)
        {
            if (playerID == 0 || playerID < 0 || playerID >= localPlayers.Count) return false;

            var p = localPlayers[playerID];
            CollectJoysticks(p.rewiredPlayer);
            p.rewiredPlayer.controllers.ClearLastActiveControllerChangedDelegates();
            p.rewiredPlayer.isPlaying = false;
            if(p.camera) Destroy(p.camera.gameObject);
            localPlayers.RemoveAt(playerID);
            OnPlayerCountChanged?.Invoke(this, localPlayers.Count+1, localPlayers.Count);
            return true;
        }

        public void SetPlayerCount(int playerCount)
        {
            if (playerCount < 1 || localPlayers.Count == playerCount) return;

            if (playerCount < localPlayers.Count)
            {
                while(localPlayers.Count != playerCount) RemovePlayer(localPlayers.Count-1);
            }
            else
            {
                while (localPlayers.Count != playerCount) AddPlayer();
            }
        }

        public int GetPlayerCount()
        {
            return localPlayers.Count;
        }

        public void CollectJoysticks()
        {
            foreach (var controller in ReInput.controllers.GetControllers(ControllerType.Joystick))
            {
                systemPlayer.rewiredPlayer.controllers.AddController(controller, true);
            }
        }

        public void CollectJoysticks(Rewired.Player rewiredPlayer)
        {
            foreach (var controller in rewiredPlayer.controllers.Controllers)
            {
                systemPlayer.rewiredPlayer.controllers.AddController(controller, true);
            }
        }

        public void GiveController(int playerID, Joystick joystick)
        {
            localPlayers[playerID].rewiredPlayer.controllers.AddController(joystick, true);
        }
        
        public void GiveController(int playerID, ControllerType type, int id)
        {
            localPlayers[playerID].rewiredPlayer.controllers.AddController(type, id, true);
        }

        public void CollectMouseKeyboard()
        {
            foreach (var p in localPlayers)
            {
                p.rewiredPlayer.controllers.hasKeyboard = false;
                p.rewiredPlayer.controllers.hasMouse = false;
            }

            systemPlayer.rewiredPlayer.controllers.hasKeyboard = true;
            systemPlayer.rewiredPlayer.controllers.hasMouse = true;
        }

        public void GiveMouseKeyboard(int playerID)
        {
            CollectMouseKeyboard();
            systemPlayer.rewiredPlayer.controllers.hasKeyboard = false;
            systemPlayer.rewiredPlayer.controllers.hasMouse = false;
            localPlayers[playerID].rewiredPlayer.controllers.hasKeyboard = true;
            localPlayers[playerID].rewiredPlayer.controllers.hasMouse = true;
        }

        public PlayerControllerType GetPlayerControllerType(int playerID)
        {
            return localPlayers[playerID].controllerType;
        }
        
        private void OnPlayerActiveControllerChanged(Player player, Controller controller)
        {
            if (controller == null)
            {
                return;
            }

            var temp = localPlayers[player.id];
            temp.controllerType =
                (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse)
                    ? PlayerControllerType.MOUSE_AND_KEYBOARD
                    : PlayerControllerType.GAMEPAD;
            localPlayers[player.id] = temp;
            OnPlayerControllerTypeChanged?.Invoke(player.id, temp.controllerType);
        }
        
        private void WhenSystemPlayerActiveControllerChanged(Player player, Controller controller)
        {
            if (controller == null)
            {
                systemPlayer.controllerType = PlayerControllerType.NONE;
                return;
            }

            var temp = systemPlayer;
            temp.controllerType =
                (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse)
                    ? PlayerControllerType.MOUSE_AND_KEYBOARD
                    : PlayerControllerType.GAMEPAD;
            systemPlayer = temp;
        }
    }
}