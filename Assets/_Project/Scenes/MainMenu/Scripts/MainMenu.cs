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

        public void Start()
        {
            usernameField.onValueChanged.AddListener(OnUsernameChanged);
            usernameField.text = $"User {UnityEngine.Random.Range(0, 1000)}";
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
            NetworkManager.singleton.StartSinglePlayerHost();
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