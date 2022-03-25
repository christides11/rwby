using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace rwby
{
    public class LocalPlayerManager : MonoBehaviour
    {
        public int maxLocalPlayers = 4;
        public List<LocalPlayerData> localPlayers = new List<LocalPlayerData>();

        public void Initialize()
        {
            AddPlayer();
        }
        
        public bool AddPlayer()
        {
            if (localPlayers.Count == maxLocalPlayers) return false;

            var rewiredPlayer = ReInput.players.GetPlayer(localPlayers.Count);
            
            localPlayers.Add(new LocalPlayerData()
            {
                controllerType = PlayerControllerType.MOUSE_AND_KEYBOARD,
                rewiredPlayer = rewiredPlayer
            });

            rewiredPlayer.isPlaying = true;
            rewiredPlayer.controllers.AddLastActiveControllerChangedDelegate(OnPlayerActiveControllerChanged);
            return true;
        }

        public bool RemovePlayer(int playerID)
        {
            if (playerID == 0 || playerID < 0 || playerID >= localPlayers.Count) return false;

            var p = localPlayers[playerID];
            p.rewiredPlayer.controllers.ClearAllControllers();
            p.rewiredPlayer.controllers.ClearLastActiveControllerChangedDelegates();
            p.rewiredPlayer.isPlaying = false;
            localPlayers.RemoveAt(playerID);
            return true;
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
        }
    }
}