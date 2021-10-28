using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;

namespace rwby.menus
{
    public class HostLobbyMenu : MonoBehaviour
    {
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private LoadingMenu loadingMenu;
        [SerializeField] private LobbyMenu LobbyMenu;

        public TMP_InputField lobbyNameTextMesh;
        public TMP_Dropdown playerCountDropdown;
        public Toggle privateLobbyToggle;

        public void OpenMenu()
        {
            gameObject.SetActive(true);
        }

        public void ExitMenu()
        {
            gameObject.SetActive(false);
        }

        public void Button_Back()
        {
            ExitMenu();
            mainMenu.gameObject.SetActive(true);
        }

        public void Button_HostLobby()
        {
            loadingMenu.OpenMenu("Attempting host...");
            int playerCount = playerCountDropdown.value + 1;

            NetworkManager.singleton.FusionLauncher.OnConnectionStatusChanged += CheckConnectionStatus;
            NetworkManager.singleton.StartHost(lobbyNameTextMesh.text, playerCount, privateLobbyToggle.isOn);
        }

        private void CheckConnectionStatus(NetworkRunner runner, FusionLauncher.ConnectionStatus status)
        {
            if (status == FusionLauncher.ConnectionStatus.Connecting) return;
            loadingMenu.CloseMenu();
            NetworkManager.singleton.FusionLauncher.OnConnectionStatusChanged -= CheckConnectionStatus;

            if (status == FusionLauncher.ConnectionStatus.Disconnected || status == FusionLauncher.ConnectionStatus.Failed) return;

            LobbyMenu.Open();
            ExitMenu();
        }
    }
}