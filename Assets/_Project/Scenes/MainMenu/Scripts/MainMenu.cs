using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using Cysharp.Threading.Tasks;

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
            lobbyMenu.Open();
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