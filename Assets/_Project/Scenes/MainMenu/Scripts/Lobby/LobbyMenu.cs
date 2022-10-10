using Fusion;
using rwby.ui.mainmenu;
using TMPro;
using UnityEngine;

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