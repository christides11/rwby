using System.Collections.Generic;
using Fusion;
using rwby.ui.mainmenu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace rwby.ui
{
    public class LobbyMenu : MenuBase
    {
        public LobbyMenuInstance lobbyMenuInstance;

        [Header("Content")] 
        public Selectable readyButton;
        public Selectable configureButton;
        public Selectable profileButton;
        public Selectable characterSelectButton;
        public Selectable settingsButton;
        public Selectable exitButton;
        public Transform characterContentTransform;
        public GameObject characterContentPrefab;
        public CharacterSelectMenu characterSelectMenu;

        [Header("Lobby Players")] 
        public Transform teamListContentHolder;
        public GameObject teamListHorizontalHolder;
        public GameObject teamHolder;
        public GameObject teamPlayerHeader;
        
        private int currentSelectingCharacterIndex = 0;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            
            readyButton.onSubmit.RemoveAllListeners();
            exitButton.onSubmit.RemoveAllListeners();
            characterSelectButton.onSubmit.RemoveAllListeners();
            readyButton.GetComponentInChildren<TextMeshProUGUI>().text = lobbyMenuInstance.lobbyMenuHandler
                .sessionManagerGamemode.Runner.IsServer ? "Start Match" : "Ready";
            if (lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.IsServer)
            {
                readyButton.GetComponent<Selectable>().onSubmit.AddListener(async () => await lobbyMenuInstance.lobbyMenuHandler.StartMatch());
            }
            else
            {
                readyButton.GetComponent<Selectable>().onSubmit.AddListener(ReadyUp);
            }
            characterSelectButton.onSubmit.AddListener(OpenCharacterSelect);
            
            Refresh();
        }

        private void ReadyUp()
        {
            PlayerRef localPlayerRef = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            var clientInfo = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.GetClientInformation(localPlayerRef);
            if (clientInfo.clientRef.IsValid == false) return;
            if (clientInfo.players.Count <= lobbyMenuInstance.playerID) return;
            ClientManager cm = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.GetPlayerObject(localPlayerRef).GetComponent<ClientManager>();
            cm.CLIENT_SetReadyStatus(!cm.ReadyStatus);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }

        public void Refresh()
        {
            PlayerRef localPlayerRef = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            var clientInfo = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.GetClientInformation(localPlayerRef);
            if (clientInfo.clientRef.IsValid == false) return;
            if (clientInfo.players.Count <= lobbyMenuInstance.playerID) return;
            ClientManager cm = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.GetPlayerObject(localPlayerRef).GetComponent<ClientManager>();

            profileButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Profile: {cm.profiles[lobbyMenuInstance.playerID]}";
            
            foreach(Transform child in characterContentTransform)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < clientInfo.players[lobbyMenuInstance.playerID].characterReferences.Count; i++)
            {
                GameObject chara = GameObject.Instantiate(characterContentPrefab, characterContentTransform, false);
                chara.GetComponentInChildren<TextMeshProUGUI>().text = "?";
                if (clientInfo.players[lobbyMenuInstance.playerID].characterReferences[i].IsValid())
                {
                    IFighterDefinition fighterDefinition = ContentManager.singleton.
                        GetContentDefinition<IFighterDefinition>(clientInfo.players[lobbyMenuInstance.playerID].characterReferences[i]);
                    chara.GetComponentInChildren<TextMeshProUGUI>().text = fighterDefinition.Name;
                }
                int selectIndex = i;
            }

            UpdatePlayerList();
        }

        private Dictionary<int, GameObject> teamHolders = new Dictionary<int, GameObject>();
        private void UpdatePlayerList()
        {
            var smg = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode;
            var playerList = smg.GetPlayerList();

            foreach (Transform child in teamListContentHolder)
            {
                Destroy(child.gameObject);
            }
            
            teamHolders.Clear();

            if (smg.teams <= 1)
            {
                var singleHorizontalHolder = GameObject.Instantiate(teamListHorizontalHolder, teamListContentHolder, false);
                singleHorizontalHolder.GetComponent<LayoutElement>().preferredHeight = 480;

                var singleTeamHolder = GameObject.Instantiate(teamHolder, singleHorizontalHolder.transform, false);
                teamHolders.Add(smg.teams, singleTeamHolder);
            }
            else
            {
                int ts = 0;
                for (int i = 0; i < (smg.teams / 4)+1; i++)
                {
                    var horizontalHolder =
                        GameObject.Instantiate(teamListHorizontalHolder, teamListContentHolder, false);
                    horizontalHolder.GetComponent<LayoutElement>().preferredHeight = 240;

                    for (int j = 0; j < 4; j++)
                    {
                        if (ts >= smg.teams) return;
                        var singleTeamHolder = GameObject.Instantiate(teamHolder, horizontalHolder.transform, false);
                        teamHolders.Add(ts+1, singleTeamHolder);
                        ts++;
                    }
                }
            }
        }

        public void OpenCharacterSelect()
        {
            characterSelectMenu.OnCharactersSelected += OnCharactersSelected;
            characterSelectMenu.charactersToSelect = 1;
            currentHandler.Forward((int)LobbyMenuType.CHARACTER_SELECT);
        }

        public void OnCharactersSelected()
        {
            characterSelectMenu.OnCharactersSelected -= OnCharactersSelected;

            PlayerRef localPlayerRef = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            var clientInfo = lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode.GetClientInformation(localPlayerRef);
            if (clientInfo.clientRef.IsValid == false) return;
            lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode
                .CLIENT_SetPlayerCharacterCount(lobbyMenuInstance.playerID, characterSelectMenu.charactersSelected.Count);

            for (int i = 0; i < characterSelectMenu.charactersSelected.Count; i++)
            {
                lobbyMenuInstance.lobbyMenuHandler.sessionManagerGamemode
                    .CLIENT_SetPlayerCharacter(lobbyMenuInstance.playerID, i,
                        characterSelectMenu.charactersSelected[i]);
            }
        }
    }
}