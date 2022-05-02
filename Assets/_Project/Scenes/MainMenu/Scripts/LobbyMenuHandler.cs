using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace rwby.ui.mainmenu
{
    // TODO: Link to given session manager.
    public class LobbyMenuHandler : MainMenuMenu
    {
        public List<LobbyMenuInstance> menuInstances = new List<LobbyMenuInstance>();
        public LobbyMenuInstance instancePrefab;
        public Transform instanceParent;

        private void OnEnable()
        {
            instancePrefab.gameObject.SetActive(false);
        }

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            ClientManager.OnPlayersChanged += WhenClientPlayerChanged;
            /*
            SessionManagerClassic.OnLobbySettingsChanged += UpdateLobbyInfo;
            SessionManagerClassic.OnGamemodeSettingsChanged += UpdateLobbyInfo;*/

            gameObject.SetActive(true);
            
            GameManager.singleton.controllerAssignmentMenu.OnControllersAssigned += OnControllersAssigned;
            GameManager.singleton.controllerAssignmentMenu.OpenMenu();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            GameManager.singleton.controllerAssignmentMenu.OnControllersAssigned -= OnControllersAssigned;
            for (int i = 0; i < menuInstances.Count; i++)
            {
                menuInstances[i].Cleanup();
                Destroy(menuInstances[i].gameObject);
            }
            menuInstances.Clear();
            
            ClientManager.OnPlayersChanged -= WhenClientPlayerChanged;
            /*SessionManagerClassic.OnLobbySettingsChanged -= UpdateLobbyInfo;
            SessionManagerClassic.OnGamemodeSettingsChanged -= UpdateLobbyInfo;*/
            gameObject.SetActive(false);
            return true;
        }

        private async UniTask StartMatch()
        {
            /*
            ClientManager.OnPlayersChanged -= WhenClientPlayerChanged;
            SessionManagerClassic.OnLobbySettingsChanged -= UpdateLobbyInfo;
            SessionManagerClassic.OnGamemodeSettingsChanged -= UpdateLobbyInfo;
            await SessionManagerClassic.singleton.TryStartMatch();*/
        }

        public async void ExitLobby()
        {
            //NetworkManager.singleton.LeaveSession();
            currentHandler.Back();
        }

        LobbyMenuInstance InitializeMenuInstance(int playerID)
        {
            LobbyMenuInstance instance = GameObject.Instantiate(instancePrefab, instanceParent, false);
            instance.canvas.worldCamera = GameManager.singleton.localPlayerManager.localPlayers[playerID].camera;
            instance.playerID = playerID;
            instance.Initialize(this);
            instance.gameObject.SetActive(true);
            return instance;
        }
        
        private void OnControllersAssigned(ControllerAssignmentMenu menu)
        {
            GameManager.singleton.controllerAssignmentMenu.CloseMenu();
            WhenLocalPlayerCountChanged(GameManager.singleton.localPlayerManager, GameManager.singleton.localPlayerManager.localPlayers.Count);
        }
        
        [SerializeField] private Camera lobbyPlayerCameraPrefab;
        private void WhenLocalPlayerCountChanged(LocalPlayerManager localplayermanager, int currentplaycount)
        {
            //ClientManager.local.CLIENT_SetPlayerCount(currentplaycount);
        }

        private void WhenClientPlayerChanged(ClientManager manager)
        {
            LocalPlayerManager localplayermanager = GameManager.singleton.localPlayerManager;
            int currentplaycount = localplayermanager.localPlayers.Count;

            if (manager.ClientPlayers.Count != currentplaycount)
            {
                localplayermanager.SetPlayerCount(manager.ClientPlayers.Count);
                currentplaycount = manager.ClientPlayers.Count;
            }
            
            while (menuInstances.Count > currentplaycount)
            {
                int i = menuInstances.Count;
                menuInstances[i].Cleanup();
                Destroy(menuInstances[i]);
                menuInstances.RemoveAt(i);
            }
            while(menuInstances.Count < currentplaycount) menuInstances.Add(null);
            
            for (int i = 0; i < currentplaycount; i++)
            {
                if (localplayermanager.localPlayers[i].camera == null)
                {
                    var temp = localplayermanager.localPlayers[i];
                    temp.camera = Instantiate(lobbyPlayerCameraPrefab, Vector3.zero, Quaternion.identity);
                    localplayermanager.localPlayers[i] = temp;
                }
                if(menuInstances[i] == null) menuInstances[i] = InitializeMenuInstance(i);
            }
            
            localplayermanager.ApplyCameraLayout();
            localplayermanager.systemPlayer.camera.enabled = false;
            
            UpdateLobbyInfo();
            UpdatePlayerInfo();
        }
        
        private void UpdateLobbyInfo()
        {
            for (int i = 0; i < menuInstances.Count; i++)
            {
                //menuInstances[i].FillGamemodeOptions(this);
            }
        }

        private void UpdatePlayerInfo()
        {
            for (int i = 0; i < menuInstances.Count; i++)
            {
                menuInstances[i].ResetCharacterList();
            }
        }
    }
}