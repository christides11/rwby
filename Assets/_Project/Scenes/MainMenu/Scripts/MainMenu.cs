using UnityEngine;
using TMPro;
using Fusion;
using Cysharp.Threading.Tasks;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine.EventSystems;

namespace rwby.menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private HostLobbyMenu hostLobbyMenu;
        [SerializeField] private FindLobbyMenu findLobbyMenu;

        public LobbyMenuHandler lobbyMenuHandler;
        public GameObject defaultSelectedUIItem;

        public GameObject joinLobbyInputMenu;
        [SerializeField] private TMP_InputField lobbyNameText;
        
        private Rewired.Player systemPlayer;
        private EventSystem eventSystem;
        public void Start()
        {
            systemPlayer = ReInput.players.GetSystemPlayer();
        }
        
        private void OnEnable()
        {
            if(LobbyManager.singleton)
            {
                lobbyMenuHandler.Open();
                return;
            }
            eventSystem = EventSystem.current;
            eventSystem.SetSelectedGameObject(defaultSelectedUIItem);
        }

        private void Update()
        {
            if (LobbyManager.singleton)
            {
                lobbyMenuHandler.Open();
                gameObject.SetActive(false);
            }

            if (eventSystem.currentSelectedGameObject == null
                && systemPlayer.GetAxis2D(rwby.Action.UIMovement_X, rwby.Action.UIMovement_Y).sqrMagnitude > 0)
            {
                eventSystem.SetSelectedGameObject(joinLobbyInputMenu.activeSelf ? lobbyNameText.gameObject : defaultSelectedUIItem);
            }
        }

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
        }
    }
}