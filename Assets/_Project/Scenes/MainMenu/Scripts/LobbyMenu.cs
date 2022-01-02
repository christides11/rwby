using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace rwby.menus
{
    public class LobbyMenu : MonoBehaviour
    {
        [SerializeField] private ContentSelect contentSelectMenu;

        [SerializeField] private TextMeshProUGUI lobbyName;
        [SerializeField] private Transform lobbyPlayerList;
        [SerializeField] private Transform teamPlayerList;
        [SerializeField] private GameObject lobbyPlayerListItem;
        [SerializeField] private GameObject teamPlayerListItem;

        [SerializeField] private Transform gamemodeOptionsList;
        [SerializeField] private GameObject gamemodeOptionsContentPrefab;

        public void Awake()
        {
            //contentSelect.OnMenuClosed += () => { gameObject.SetActive(true); };
            //MatchManager.onMatchSettingsLoadSuccess += MatchSettingsLoadSuccess;
            //MatchManager.onMatchSettingsLoadFailed += MatchSettingsLoadFailed;
        }

        public void Open()
        {
            LobbyManager.OnLobbySettingsChanged += UpdateLobbyInfo;
            lobbyName.text = NetworkManager.singleton.FusionLauncher.NetworkRunner.SessionInfo.Name;
            FillTeamList();
            FillLobbyPlayerList();
            UpdateLobbyInfo();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            LobbyManager.OnLobbySettingsChanged -= UpdateLobbyInfo;
            gameObject.SetActive(false);
        }

        private void UpdateLobbyInfo()
        {
            FillGamemodeOptions();
        }

        private void FillTeamList()
        {
            foreach(Transform child in teamPlayerList)
            {
                Destroy(child.gameObject);
            }
        }

        private void FillLobbyPlayerList()
        {
            foreach(Transform child in lobbyPlayerList)
            {
                Destroy(child.gameObject);
            }
        }

        private void FillGamemodeOptions()
        {
            foreach(Transform child in gamemodeOptionsList)
            {
                Destroy(child.gameObject);
            }

            GameObject gamemodeOb = GameObject.Instantiate(gamemodeOptionsContentPrefab, gamemodeOptionsList, false);
            TextMeshProUGUI[] textMeshes = gamemodeOb.GetComponentsInChildren<TextMeshProUGUI>();
            textMeshes[0].text = LobbyManager.singleton.Settings.gamemodeReference;
            PlayerPointerEventTrigger ppet = gamemodeOb.GetComponentInChildren<PlayerPointerEventTrigger>();
            ppet.OnPointerClickEvent.AddListener((d) => { _ = OpenGamemodeSelection(); });
            
            if (LobbyManager.singleton.CurrentGameMode == null) return;
            //LobbyManager.singleton.CurrentGameMode.AddGamemodeSettings(this);
        }

        private async UniTask OpenGamemodeSelection()
        {
            await contentSelectMenu.OpenMenu<IGameModeDefinition>((a, b) => { OnGamemodeSelection(b); });
        }

        private async void OnGamemodeSelection(ModObjectReference gamemodeReference)
        {
            contentSelectMenu.CloseMenu();
            await LobbyManager.singleton.TrySetGamemode(gamemodeReference);
        }
    }
}