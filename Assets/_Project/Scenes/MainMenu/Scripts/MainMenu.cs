using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

namespace rwby.menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private LobbyMenu LobbyMenu;

        [SerializeField] private TMP_InputField lobbyNameText;
        [SerializeField] private TMP_InputField usernameField;

        public void Start()
        {
            if(NetworkManager.singleton.FusionLauncher.Status == FusionLauncher.ConnectionStatus.Connected)
            {
                LobbyMenu.Open();
                gameObject.SetActive(false);
            }
            NetworkManager.singleton.FusionLauncher.ClientOnConnectedToServer += OnClientConnected;
            NetworkManager.singleton.FusionLauncher.OnStartHosting += OnLobbyJoined;
            usernameField.onValueChanged.AddListener(OnUsernameChanged);
            usernameField.text = $"User {UnityEngine.Random.Range(0, 1000)}";
        }

        private void OnUsernameChanged(string arg0)
        {
            GameManager.singleton.localUsername = arg0;
        }

        private void OnClientConnected(NetworkRunner runner)
        {
            OnLobbyJoined();
        }

        public void ButtonHostLobby()
        {
            NetworkManager.singleton.StartHost();
        }

        public void ButtonSingleplayer()
        {
            NetworkManager.singleton.StartSinglePlayerHost();
        }

        public void ButtonFindLobby()
        {

        }

        public void ButtonJoinLobby()
        {
            NetworkManager.singleton.JoinHost(lobbyNameText.text);
        }

        private void OnLobbyJoined()
        {
            LobbyMenu.Open();
            gameObject.SetActive(false);
        }
    }
}