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
        [SerializeField] private HostLobbyMenu hostLobbyMenu;
        [SerializeField] private FindLobbyMenu findLobbyMenu;

        [SerializeField] private TMP_InputField lobbyNameText;
        [SerializeField] private TMP_InputField usernameField;

        public LobbyMenu lobbyMenu;

        public void Start()
        {
            usernameField.onValueChanged.AddListener(OnUsernameChanged);
            usernameField.text = $"User {UnityEngine.Random.Range(0, 1000)}";
        }

        private void OnEnable()
        {
            if(LobbyManager.singleton)
            {
                lobbyMenu.Open();
            }
        }

        private void Update()
        {
            if (LobbyManager.singleton)
            {
                lobbyMenu.Open();
                gameObject.SetActive(false);
            }
        }

        private void OnUsernameChanged(string arg0)
        {
            GameManager.singleton.localUsername = arg0;
        }

        public void ButtonHostLobby()
        {
            hostLobbyMenu.OpenMenu();
            gameObject.SetActive(false);
        }

        public void ButtonSingleplayer()
        {
            NetworkManager.singleton.FusionLauncher.OnHostingFailed += OnHostingFailed;
            NetworkManager.singleton.FusionLauncher.OnStartHosting += OnHostingSuccess;
            NetworkManager.singleton.StartSinglePlayerHost();
        }

        private void OnHostingSuccess()
        {
            NetworkManager.singleton.FusionLauncher.OnStartHosting -= OnHostingSuccess;
            //loadingMenu.CloseMenu();
            gameObject.SetActive(false);
            lobbyMenu.Open();
        }

        private void OnHostingFailed()
        {
            NetworkManager.singleton.FusionLauncher.OnHostingFailed -= OnHostingFailed;
            //loadingMenu.CloseMenu();
        }

        public void ButtonFindLobby()
        {
            findLobbyMenu.OpenMenu();
            gameObject.SetActive(false);
        }

        public void ButtonJoinLobby()
        {
            NetworkManager.singleton.JoinHost(lobbyNameText.text);
        }
    }
}