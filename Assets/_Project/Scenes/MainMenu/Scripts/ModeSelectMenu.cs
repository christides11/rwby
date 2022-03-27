using System;
using UnityEngine;
using TMPro;
using Fusion;
using Cysharp.Threading.Tasks;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine.EventSystems;

namespace rwby.menus
{
    public class ModeSelectMenu : MonoBehaviour
    {
        [SerializeField] private HostLobbyMenu hostLobbyMenu;
        [SerializeField] private FindLobbyMenu findLobbyMenu;

        public LobbyMenuHandler lobbyMenuHandler;
        public GameObject defaultSelectedUIItem;

        public GameObject joinLobbyInputMenu;
        [SerializeField] private TMP_InputField lobbyNameText;
        
        private Rewired.Player systemPlayer;
        private EventSystem eventSystem;
        private LocalPlayerManager localPlayerManager;

        [Header("Menus")] public OnlineMenu onlineMenu;
        
        private void Start()
        {
            localPlayerManager = GameManager.singleton.localPlayerManager;
            Open();
        }

        public void Open()
        {
            if(LobbyManager.singleton)
            {
                lobbyMenuHandler.Open();
                return;
            }
            systemPlayer = ReInput.players.GetSystemPlayer();
            eventSystem = EventSystem.current;
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (LobbyManager.singleton)
            {
                lobbyMenuHandler.Open();
                gameObject.SetActive(false);
            }

            
            if (eventSystem.currentSelectedGameObject == null
                && localPlayerManager.systemPlayer.controllerType == PlayerControllerType.GAMEPAD
                && systemPlayer.GetAxis2D(rwby.Action.UIMovement_X, rwby.Action.UIMovement_Y).sqrMagnitude > 0)
            {
                eventSystem.SetSelectedGameObject(joinLobbyInputMenu.activeSelf ? lobbyNameText.gameObject : defaultSelectedUIItem);
            }
        }

        public void BUTTON_Online()
        {
            onlineMenu.Open();
            Close();
        }

        public void BUTTON_Local()
        {
            Close();
        }

        public void BUTTON_Modding()
        {
            Close();
        }
        
        public void BUTTON_Options()
        {
            Close();
        }

        public void BUTTON_Exit()
        {
            Application.Quit();
        }
        
        /*
        public void ButtonHostLobby()
        {
            hostLobbyMenu.OpenMenu();
            gameObject.SetActive(false);
        }

        public void ButtonSingleplayer()
        {
            _ = TryStartSingleplayer();
        }

        private async UniTaskVoid TryStartSingleplayer()
        {
            StartGameResult result = await NetworkManager.singleton.StartSinglePlayerHost();
            if(result.Ok == false)
            {
                return;
            }
            gameObject.SetActive(false);

            while(LobbyManager.singleton == null)
            {
                await UniTask.WaitForFixedUpdate();
            }
            lobbyMenuHandler.Open();
        }

        public void ButtonFindLobby()
        {
            findLobbyMenu.OpenMenu();
            gameObject.SetActive(false);
        }

        public void ButtonJoinLobby()
        {
            joinLobbyInputMenu.SetActive(true);
            eventSystem.SetSelectedGameObject(lobbyNameText.gameObject);
        }

        public void TryJoinLobby()
        {
            NetworkManager.singleton.JoinHost(lobbyNameText.text);
            joinLobbyInputMenu.SetActive(false);
        }*/
    }
}