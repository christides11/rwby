using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using Rewired;
using Rewired.Integration.UnityUI;
using System;

namespace rwby.menus
{
    public class LobbyMenuHandler : MonoBehaviour
    {
        public List<LobbyMenuInstance> menuInstances = new List<LobbyMenuInstance>();
        public LobbyMenuInstance instancePrefab;
        public Transform instanceParent;
        
        /*
        [SerializeField] private TextMeshProUGUI lobbyName;
        [SerializeField] private Transform lobbyPlayerList;
        [SerializeField] private Transform localPlayerList;
        [SerializeField] private GameObject lobbyPlayerListItem;
        [SerializeField] private GameObject localPlayerListItem;
        [SerializeField] private GameObject localPlayerListAddPlayerItem;

        [SerializeField] public Transform gamemodeOptionsList;
        [SerializeField] public GameObject gamemodeOptionsContentPrefab;

        [SerializeField] private Button startMatchButton;*/

        private ClientManager.ClientAction storedAction;
        
        public void Open()
        {
            //storedAction = (a) => { UpdatePlayerInfo(); };
            //ClientManager.OnPlayersChanged += storedAction;
            LobbyManager.OnLobbySettingsChanged += UpdateLobbyInfo;
            LobbyManager.OnGamemodeSettingsChanged += UpdateLobbyInfo;
            
            /*
            startMatchButton.GetComponentInChildren<PlayerPointerEventTrigger>().OnPointerClickEvent.AddListener(async (a) => { await StartMatch(); } );
            UpdatePlayerInfo();
            UpdateLobbyInfo();
            */
            
            gameObject.SetActive(true);
            
            WhenPlayerCountChanged(GameManager.singleton.localPlayerManager, 1);
            InitializeMenuInstance(0);
            GameManager.singleton.controllerAssignmentMenu.OnControllersAssigned += OnControllersAssigned;
            GameManager.singleton.controllerAssignmentMenu.OpenMenu();
        }

        public void Close()
        {
            GameManager.singleton.controllerAssignmentMenu.OnControllersAssigned -= OnControllersAssigned;
            for (int i = 0; i < menuInstances.Count; i++)
            {
                menuInstances[i].Cleanup();
                Destroy(menuInstances[i].gameObject);
            }
            menuInstances.Clear();
            /*
            startMatchButton.GetComponentInChildren<PlayerPointerEventTrigger>().OnPointerClickEvent.RemoveAllListeners();
            ClientManager.OnPlayersChanged -= storedAction;
            LobbyManager.OnLobbySettingsChanged -= UpdateLobbyInfo;
            LobbyManager.OnGamemodeSettingsChanged -= UpdateLobbyInfo;
            gameObject.SetActive(false);
            */
        }
        
        private async UniTask StartMatch()
        {
            /*
            ClientManager.OnPlayersChanged -= storedAction;
            LobbyManager.OnLobbySettingsChanged -= UpdateLobbyInfo;
            LobbyManager.OnGamemodeSettingsChanged -= UpdateLobbyInfo;
            
            await LobbyManager.singleton.TryStartMatch();*/
        }

        void InitializeMenuInstance(int playerID)
        {
            LobbyMenuInstance instance = GameObject.Instantiate(instancePrefab, instanceParent, false);
            instance.canvas.worldCamera = GameManager.singleton.localPlayerManager.localPlayers[playerID].camera;
            instance.lobbyName.text = NetworkManager.singleton.FusionLauncher.NetworkRunner.SessionInfo.Name;
            instance.playerID = playerID;
            instance.Initialize(this);
            instance.gameObject.SetActive(true);
        }
        
        private void OnControllersAssigned(ControllerAssignmentMenu menu)
        {
            GameManager.singleton.controllerAssignmentMenu.CloseMenu();
            WhenPlayerCountChanged(GameManager.singleton.localPlayerManager, GameManager.singleton.localPlayerManager.localPlayers.Count);
        }
        
        [SerializeField] private Camera lobbyPlayerCameraPrefab;
        private void WhenPlayerCountChanged(LocalPlayerManager localplayermanager, int currentplaycount)
        {
            for (int i = 0; i < currentplaycount; i++)
            {
                if (localplayermanager.localPlayers[i].camera == null)
                {
                    var temp = localplayermanager.localPlayers[i];
                    temp.camera = Instantiate(lobbyPlayerCameraPrefab, Vector3.zero, Quaternion.identity);
                    localplayermanager.localPlayers[i] = temp;
                }
                InitializeMenuInstance(i);
            }
            
            localplayermanager.ApplyCameraLayout();
            localplayermanager.systemPlayer.camera.enabled = false;
        }
        
        private void UpdateLobbyInfo()
        {
            for (int i = 0; i < menuInstances.Count; i++)
            {
                menuInstances[i].FillGamemodeOptions(this);
            }
        }

        private void UpdatePlayerInfo()
        {
            for (int i = 0; i < menuInstances.Count; i++)
            {
                menuInstances[i].FillLobbyPlayerList();
                menuInstances[i].FillPlayerCharacterList();
            }
        }
    }
}