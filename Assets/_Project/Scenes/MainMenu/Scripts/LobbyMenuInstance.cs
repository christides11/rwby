using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Rewired;
using Rewired.Integration.UnityUI;
using rwby.menus;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace rwby
{
    public class LobbyMenuInstance : MonoBehaviour
    {
        public Canvas canvas;
        public TextMeshProUGUI lobbyName;
        public Transform lobbyPlayerList;
        public GameObject lobbyPlayerListItem;
        
        public GameObject playerCharacterList;
        public GameObject playerCharacterListItem;
        public GameObject playerCharacterAddItem;
        
        [SerializeField] public Transform gamemodeOptionsList;
        [SerializeField] public GameObject gamemodeOptionsContentPrefab;
        
        public Button startMatchButton;
        public DropDownList teamSelectDropdown;
        
        [Header("Player List")]
        public GameObject lobbyPlayerTeamItem;

        public int playerID;

        private LobbyMenuHandler lobbyMenuHandler;
        
        public void Initialize(LobbyMenuHandler menuHandler)
        {
            this.lobbyMenuHandler = menuHandler;
        }
        
        public void Cleanup()
        {
            startMatchButton.GetComponent<EventTrigger>().RemoveAllListeners();
        }

        public void FillGamemodeOptions(LobbyMenuHandler handler)
        {
            foreach(Transform child in gamemodeOptionsList)
            {
                Destroy(child.gameObject);
            }
            teamSelectDropdown.ResetItems();

            GameObject gamemodeOb = GameObject.Instantiate(gamemodeOptionsContentPrefab, gamemodeOptionsList, false);
            TextMeshProUGUI[] textMeshes = gamemodeOb.GetComponentsInChildren<TextMeshProUGUI>();
            textMeshes[0].text = LobbyManager.singleton.Settings.gamemodeReference.ToString();
            gamemodeOb.GetComponent<EventTrigger>().AddOnSubmitListeners(async (a) => { await OpenGamemodeSelection(); });
            
            if (LobbyManager.singleton.CurrentGameMode == null) return;
            LobbyManager.singleton.CurrentGameMode.AddGamemodeSettings(handler);

            var gamemode = LobbyManager.singleton.CurrentGameMode;
            for (int i = 0; i < LobbyManager.singleton.Settings.teams; i++)
            {
                teamSelectDropdown.AddItem(new DropDownListItem(){ Caption = gamemode.definition.teams[i].teamName, ID = $"{i}" });
            }
        }
        
        Dictionary<byte, Transform> teamContainers = new Dictionary<byte, Transform>();
        public void FillLobbyPlayerList()
        {
            foreach(Transform child in lobbyPlayerList)
            {
                Destroy(child.gameObject);
            }

            teamContainers.Clear();

            ModObjectReference gamemodeRef = LobbyManager.singleton.Settings.gamemodeReference;
            IGameModeDefinition ll;
            if(gamemodeRef.IsValid()) {
                ll = ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(gamemodeRef);

                for (int i = 0; i < LobbyManager.singleton.Settings.teams; i++)
                {
                    GameObject teamContainer = GameObject.Instantiate(lobbyPlayerTeamItem, lobbyPlayerList, false);
                    teamContainers.Add((byte)(i+1), teamContainer.transform);
                }
            }

            for(int j = 0; j < ClientManager.clientManagers.Count; j++)
            {
                for (int k = 0; k < ClientManager.clientManagers[j].ClientPlayers.Count; k++)
                {
                    GameObject playerItem;
                    if (ClientManager.clientManagers[j].ClientPlayers[k].team == 0)
                    {
                        playerItem = GameObject.Instantiate(lobbyPlayerListItem, lobbyPlayerList, false);
                    }
                    else
                    {
                        playerItem = GameObject.Instantiate(lobbyPlayerListItem, teamContainers[ClientManager.clientManagers[j].ClientPlayers[k].team].transform, false);
                    }
                }
            }

            foreach(var v in teamContainers)
            {
                if(v.Value.childCount == 0)
                {
                    Destroy(v.Value.gameObject);
                }
            }
        }
        
        public void FillPlayerCharacterList()
        {
            foreach(Transform child in playerCharacterList.transform)
            {
                Destroy(child.gameObject);
            }

            if (ClientManager.local == null) return;

            for(int i = 0; i < ClientManager.local.ClientPlayers.Count; i++)
            {
                GameObject playerItem = GameObject.Instantiate(playerCharacterListItem, playerCharacterList.transform, false);
                TextMeshProUGUI[] textMeshes = playerItem.GetComponentsInChildren<TextMeshProUGUI>();
                textMeshes[0].text = ClientManager.local.ClientPlayers[i].characterReference.ToString();
                playerItem.GetComponent<EventTrigger>().AddOnSubmitListeners((d) => { OpenCharacterSelection(); });
            }

            if (ClientManager.local.ClientPlayers.Count == 4) return;

            GameObject playerAddItem = GameObject.Instantiate(playerCharacterAddItem, playerCharacterList.transform, false);
            playerAddItem.GetComponent<EventTrigger>().AddOnSubmitListeners((d) => { AddPlayerCharacter(); });
        }

        private void AddPlayerCharacter()
        {
            //ClientManager.local.CLIENT_SetPlayerCharacterCount(playerID, ClientManager.local.ClientPlayers[playerID].characterReferences.Count + 1);
        }

        private void ChangePlayerTeam(PlayerPointerEventData a)
        {
            byte currentTeam = ClientManager.local.ClientPlayers[playerID].team;

            currentTeam++;
            if(currentTeam > LobbyManager.singleton.Settings.teams)
            {
                currentTeam = 0;
            }

            ClientManager.local.CLIENT_SetPlayerTeam(playerID, currentTeam);
        }
        
        private void OpenCharacterSelection()
        {
            //_ = ContentSelect.singleton.OpenMenu<IFighterDefinition>((a, b) => { OnCharacterSelection(a, b); });
        }

        private void OnCharacterSelection(PlayerPointerEventData a, ModObjectReference b)
        {
            //ContentSelect.singleton.CloseMenu();
            ClientManager.local.CLIENT_SetPlayerCharacter(playerID, b);
        }

        private async UniTask OpenGamemodeSelection()
        {
            //await ContentSelect.singleton.OpenMenu<IGameModeDefinition>((a, b) => { OnGamemodeSelection(b); });
        }

        private async void OnGamemodeSelection(ModObjectReference gamemodeReference)
        {
            //ContentSelect.singleton.CloseMenu();
            await LobbyManager.singleton.TrySetGamemode(gamemodeReference);
        }
    }
}