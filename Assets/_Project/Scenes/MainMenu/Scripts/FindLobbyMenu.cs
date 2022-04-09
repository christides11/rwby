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
        [SerializeField] private OnlineMenu onlineMenu;
        [SerializeField] private LobbyMenuHandler lobbyMenuHandler;

        [Header("UI")] 
        public GameObject defaultSelectable;
        public TextMeshProUGUI pageText;

        private CancellationTokenSource refreshLobbiesCancelToken = new CancellationTokenSource();

        private int page = 0;

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            NetworkManager.singleton.FusionLauncher.OnSessionsUpdated += OnSessionsUpdated;
            _ = NetworkManager.singleton.FusionLauncher.JoinSessionLobby();
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            NetworkManager.singleton.FusionLauncher.OnSessionsUpdated -= OnSessionsUpdated;
            ClearLobbyScrollView();
            gameObject.SetActive(false);
            return true;
        }

        private void OnDisable()
        {
            NetworkManager.singleton.FusionLauncher.OnSessionsUpdated -= OnSessionsUpdated;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                currentHandler.Back();
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
                lci.serverName.text = session.Name;
                lci.players.text = "0/0";//$"{session.PlayerCount}/{session.MaxPlayers}";
                lci.ping.text = "0";
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
            //loadingMenu.OpenMenu("Attempting to connect...");

            NetworkManager.singleton.FusionLauncher.OnConnectionStatusChanged += CheckConnectionStatus;
            NetworkManager.singleton.JoinHost(session);
            GameManager.singleton.loadingMenu.OpenMenu(0, "Joining Lobby...");
        }

        private void CheckConnectionStatus(NetworkRunner runner, FusionLauncher.ConnectionStatus status)
        {
            if (status == FusionLauncher.ConnectionStatus.Connecting) return;
            GameManager.singleton.loadingMenu.CloseMenu(0);
            NetworkManager.singleton.FusionLauncher.OnConnectionStatusChanged -= CheckConnectionStatus;

            if (status == FusionLauncher.ConnectionStatus.Disconnected || status == FusionLauncher.ConnectionStatus.Failed) return;

            currentHandler.Forward((int)MainMenuType.LOBBY);
        }
    }
}