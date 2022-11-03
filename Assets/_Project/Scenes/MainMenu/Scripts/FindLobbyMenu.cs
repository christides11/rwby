using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.EventSystems;

namespace rwby.ui.mainmenu
{
    public class FindLobbyMenu : MainMenuMenu
    {
        [SerializeField] private Transform LobbyContentHolder;
        [SerializeField] private FindLobbyMenuContent lobbyContentItem;

        [Header("Menus")] 
        [SerializeField] private MainMenu mainMenu;
        //[SerializeField] private OnlineMenu onlineMenu;
        [SerializeField] private LobbyMenuHandler lobbyMenuHandler;

        [Header("UI")] 
        public GameObject defaultSelectable;
        public TextMeshProUGUI pageText;

        private CancellationTokenSource refreshLobbiesCancelToken = new CancellationTokenSource();

        private int page = 0;

        private int sessionHandlerID = -1;
        private FusionLauncher sessionHandler;

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
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
                lci.serverName.text = session.Name;
                lci.players.text = "0/0";//$"{session.PlayerCount}/{session.MaxPlayers}";
                lci.ping.text = "0";
                lci.selectable.onSubmit.AddListener(async () => { await Button_JoinLobby(session); });
            }
        }

        protected void ClearLobbyScrollView()
        {
            foreach (Transform child in LobbyContentHolder)
            {
                Destroy(child.gameObject);
            }
        }
        
        public async UniTask Button_JoinLobby(SessionInfo session)
        {
            GameManager.singleton.loadingMenu.OpenMenu(0, "Attempting to connect...");

            var result = await sessionHandler.JoinSession(session);
            GameManager.singleton.loadingMenu.CloseMenu(0);
            if (!result.Ok)
            {
                return;
            }
            
            await UniTask.WaitUntil(() => sessionHandler.sessionManager != null);
            lobbyMenuHandler.sessionManagerGamemode = (SessionManagerGamemode)sessionHandler.sessionManager;
            
            mainMenu.currentHandler.Forward((int)MainMenuType.LOBBY);
        }
    }
}