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

        public void Awake()
        {
            //contentSelect.OnMenuClosed += () => { gameObject.SetActive(true); };
            //MatchManager.onMatchSettingsLoadSuccess += MatchSettingsLoadSuccess;
            //MatchManager.onMatchSettingsLoadFailed += MatchSettingsLoadFailed;
        }

        public void Open()
        {
            lobbyName.text = NetworkManager.singleton.FusionLauncher.NetworkRunner.SessionInfo.Name;
            FillTeamList();
            FillLobbyPlayerList();
            FillGamemodeOptions();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
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

        }
    }
}