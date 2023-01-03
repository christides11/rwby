using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby.ui.mainmenu
{
    public class FindLobbyMenu : MainMenuMenu
    {
        public CanvasGroup[] canvasGroups;
        
        [SerializeField] private Transform LobbyContentHolder;
        [SerializeField] private FindLobbyMenuContent lobbyContentItem;

        private EventSystem eventSystem;
        private LocalPlayerManager localPlayerManager;
        
        [Header("Menus")] 
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private LobbyMenuHandler lobbyMenuHandler;

        [Header("UI")] 
        public GameObject defaultSelectable;
        public TextMeshProUGUI pageText;
        
        [Header("Password Entry")]
        public GameObject passwordEntryMenu;
        public TMP_InputField passwordEntryInputField;
        public rwby.ui.Selectable passwordEntryConfirm;
        public rwby.ui.Selectable passwordEntryBack;

        private CancellationTokenSource refreshLobbiesCancelToken = new CancellationTokenSource();

        private int page = 0;

        private int sessionHandlerID = -1;
        private FusionLauncher sessionHandler;

        private void Awake()
        {
            localPlayerManager = GameManager.singleton.localPlayerManager;
        }
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            eventSystem = EventSystem.current;
            sessionHandlerID = NetworkManager.singleton.CreateSessionHandler();
            sessionHandler = NetworkManager.singleton.GetSessionHandler(sessionHandlerID);
            sessionHandler.OnSessionsUpdated += OnSessionsUpdated;
            _ = sessionHandler.JoinSessionLobby();
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            if(sessionHandler) sessionHandler.OnSessionsUpdated -= OnSessionsUpdated;
            if (direction == MenuDirection.BACKWARDS)
            {
                NetworkManager.singleton.DestroySessionHandler(sessionHandlerID);
            }
            ClearLobbyScrollView();
            gameObject.SetActive(false);
            return true;
        }
        
        private void Update()
        {
            if (UIHelpers.SelectDefaultSelectable(eventSystem, localPlayerManager.localPlayers[0]))
            {
                if (canvasGroups[0].interactable)
                {
                    if (LobbyContentHolder.childCount != 0)
                    {
                        eventSystem.SetSelectedGameObject(LobbyContentHolder.GetChild(0).gameObject);
                    }
                    else
                    {
                        eventSystem.SetSelectedGameObject(defaultSelectable);
                    }
                }
                else
                {
                    eventSystem.SetSelectedGameObject(passwordEntryConfirm.gameObject);
                }
            }
        }

        private void OnSessionsUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            PopulateLobbyScrollView(sessionList);
        }

        protected void PopulateLobbyScrollView(List<SessionInfo> sessionList)
        {
            ClearLobbyScrollView();

            for(int i = 0; i < sessionList.Count; i++)
            {
                SessionInfo session = sessionList[i];
                FindLobbyMenuContent lci = GameObject.Instantiate(lobbyContentItem, LobbyContentHolder, false);
                lci.serverName.text = session.Properties["name"];
                lci.map.text = session.Properties["map"];
                lci.gamemode.text = session.Properties["gamemode"];
                lci.playerCount.text = session.PlayerCount.ToString();
                lci.playerLimit.text = session.MaxPlayers.ToString();
                lci.password.color = session.Properties["password"] == 0 ? Color.gray : Color.white;
                lci.region.text = session.Region.ToString();
                lci.selectable.onSubmit.AddListener(() => { Button_JoinLobby(session); });
            }
        }

        protected void ClearLobbyScrollView()
        {
            foreach (Transform child in LobbyContentHolder)
            {
                Destroy(child.gameObject);
            }
        }
        
        public void Button_JoinLobby(SessionInfo session)
        {
            if (session.Properties["password"] == 1)
            {
                OpenPasswordEntry(session);
                return;
            }

            _ = TryJoinLobby(session);
        }

        private SessionInfo currentPasswordedSession;
        private void OpenPasswordEntry(SessionInfo session)
        {
            currentPasswordedSession = session;
            passwordEntryMenu.SetActive(true);
            passwordEntryConfirm.onSubmit.RemoveAllListeners();
            passwordEntryBack.onSubmit.RemoveAllListeners();
            passwordEntryConfirm.onSubmit.AddListener(() => { TryJoinLobbyWithPassword(); });
            passwordEntryBack.onSubmit.AddListener(() => { ClosePasswordEntry(); });

            foreach (var cg in canvasGroups)
            {
                cg.interactable = false;
            }
        }

        private void ClosePasswordEntry()
        {
            passwordEntryMenu.SetActive(false);
        }
        
        private void TryJoinLobbyWithPassword()
        {
            ClosePasswordEntry();
            _ = TryJoinLobby(currentPasswordedSession);
        }

        async UniTask TryJoinLobby(SessionInfo session)
        {
            foreach (var cg in canvasGroups)
            {
                cg.interactable = true;
            }
            GameManager.singleton.loadingMenu.OpenMenu(0, "Attempting to connect...");

            var result = await sessionHandler.JoinSession(session, passwordEntryInputField.text);
            if (!result.Ok)
            {
                GameManager.singleton.loadingMenu.CloseMenu(0);
                return;
            }
            
            await UniTask.WaitUntil(() => sessionHandler.sessionManager != null);
            GameManager.singleton.loadingMenu.CloseMenu(0);
            lobbyMenuHandler.sessionManagerGamemode = (SessionManagerGamemode)sessionHandler.sessionManager;
            
            mainMenu.currentHandler.Forward((int)MainMenuType.LOBBY);
        }
    }
}