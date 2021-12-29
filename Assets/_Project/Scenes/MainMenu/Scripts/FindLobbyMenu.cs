using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.EventSystems;

namespace rwby.menus
{
    public class FindLobbyMenu : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI lobbyNameTextMesh;
        [SerializeField] private LoadingMenu loadingMenu;
        [SerializeField] private Transform LobbyContentHolder;
        [SerializeField] private GameObject lobbyContentItem;

        [Header("Menus")]
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private LobbyMenu lobbyMenu;

        private CancellationTokenSource refreshLobbiesCancelToken = new CancellationTokenSource();
        private SessionInfo currentlyViewingLobby = null;

        public void OpenMenu()
        {
            NetworkManager.singleton.FusionLauncher.OnSessionsUpdated += OnSessionsUpdated;
            _ = NetworkManager.singleton.FusionLauncher.JoinSessionLobby();
            gameObject.SetActive(true);
        }

        public void CloseMenu()
        {
            NetworkManager.singleton.FusionLauncher.OnSessionsUpdated -= OnSessionsUpdated;
            ClearLobbyScrollView();
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            NetworkManager.singleton.FusionLauncher.OnSessionsUpdated -= OnSessionsUpdated;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                mainMenu.gameObject.SetActive(true);
                CloseMenu();
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
                GameObject lci = GameObject.Instantiate(lobbyContentItem, LobbyContentHolder, false);
                lci.GetComponentInChildren<TextMeshProUGUI>().text = session.Name;

                lci.GetComponent<EventTrigger>().AddOnSubmitListeners((data) => { SetViewingLobby(session); });
            }
        }

        private void SetViewingLobby(SessionInfo session)
        {
            currentlyViewingLobby = session;
            lobbyNameTextMesh.text = session.Name;
        }

        protected void ClearLobbyScrollView()
        {
            foreach (Transform child in LobbyContentHolder)
            {
                Destroy(child.gameObject);
            }
        }

        public void Button_JoinLobby()
        {
            if (currentlyViewingLobby == null) return;

            loadingMenu.OpenMenu("Attempting to connect...");

            NetworkManager.singleton.FusionLauncher.OnConnectionStatusChanged += CheckConnectionStatus;
            NetworkManager.singleton.JoinHost(currentlyViewingLobby);
        }

        private void CheckConnectionStatus(NetworkRunner runner, FusionLauncher.ConnectionStatus status)
        {
            if (status == FusionLauncher.ConnectionStatus.Connecting) return;
            loadingMenu.CloseMenu();
            NetworkManager.singleton.FusionLauncher.OnConnectionStatusChanged -= CheckConnectionStatus;

            if (status == FusionLauncher.ConnectionStatus.Disconnected || status == FusionLauncher.ConnectionStatus.Failed) return;

            lobbyMenu.Open();
            CloseMenu();
        }
    }
}