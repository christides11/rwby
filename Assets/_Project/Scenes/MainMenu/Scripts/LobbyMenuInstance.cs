using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Rewired;
using Rewired.Integration.UnityUI;
using rwby.menus;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        
        [SerializeField] private Button startMatchButton;
        
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
            
        }
        
        public void FillGamemodeOptions(LobbyMenuHandler handler)
        {
            foreach(Transform child in gamemodeOptionsList)
            {
                Destroy(child.gameObject);
            }

            GameObject gamemodeOb = GameObject.Instantiate(gamemodeOptionsContentPrefab, gamemodeOptionsList, false);
            TextMeshProUGUI[] textMeshes = gamemodeOb.GetComponentsInChildren<TextMeshProUGUI>();
            textMeshes[0].text = LobbyManager.singleton.Settings.gamemodeReference.ToString();
            PlayerPointerEventTrigger ppet = gamemodeOb.GetComponentInChildren<PlayerPointerEventTrigger>();
            //ppet.OnPointerClickEvent.AddListener((d) => { _ = OpenGamemodeSelection(); });
            
            if (LobbyManager.singleton.CurrentGameMode == null) return;
            LobbyManager.singleton.CurrentGameMode.AddGamemodeSettings(handler);
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
                playerItem.GetComponentInChildren<PlayerPointerEventTrigger>().OnPointerClickEvent.AddListener((d) => { OpenCharacterSelection(); });

                if (ClientManager.local.ClientPlayers[i].team == 0)
                {
                    playerItem.GetComponentsInChildren<TextMeshProUGUI>()[1].text = "No Team";
                }
                else
                {
                    playerItem.GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"Team {ClientManager.local.ClientPlayers[i].team}";
                }

                playerItem.GetComponentsInChildren<PlayerPointerEventTrigger>()[1].OnPointerClickEvent.AddListener((a) => { ChangePlayerTeam(a); });
            }

            if (ClientManager.local.ClientPlayers.Count == 4) return;

            GameObject playerAddItem = GameObject.Instantiate(playerCharacterAddItem, playerCharacterList.transform, false);
            PlayerPointerEventTrigger ppet = playerAddItem.GetComponentInChildren<PlayerPointerEventTrigger>();
            ppet.OnPointerClickEvent.AddListener((d) => { ClientManager.local.AddPlayer(ReInput.players.GetPlayer(d.playerId)); });
        }

        private void ChangePlayerTeam(PlayerPointerEventData a)
        {
            int localPlayer = ClientManager.local.GetPlayerIndex(ReInput.players.GetPlayer(a.playerId));
            byte currentTeam = ClientManager.local.ClientPlayers[localPlayer].team;

            currentTeam++;
            if(currentTeam > LobbyManager.singleton.Settings.teams)
            {
                currentTeam = 0;
            }

            ClientManager.local.SetPlayerTeam(localPlayer, currentTeam);
        }
        
        private void OpenCharacterSelection()
        {
            //_ = ContentSelect.singleton.OpenMenu<IFighterDefinition>((a, b) => { OnCharacterSelection(a, b); });
        }

        private void OnCharacterSelection(PlayerPointerEventData a, ModObjectReference b)
        {
            ContentSelect.singleton.CloseMenu();
            ClientManager.local.SetPlayerCharacter(ReInput.players.GetPlayer(a.playerId), b);
        }

        private async UniTask OpenGamemodeSelection()
        {
            await ContentSelect.singleton.OpenMenu<IGameModeDefinition>((a, b) => { OnGamemodeSelection(b); });
        }

        private async void OnGamemodeSelection(ModObjectReference gamemodeReference)
        {
            ContentSelect.singleton.CloseMenu();
            await LobbyManager.singleton.TrySetGamemode(gamemodeReference);
        }
    }
}