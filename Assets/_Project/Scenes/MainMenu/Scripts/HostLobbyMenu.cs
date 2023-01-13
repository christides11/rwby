using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace rwby.ui.mainmenu
{
    // TODO: Unload temporary gamemode when going backwards.
    public class HostLobbyMenu : MainMenuMenu
    {
        [SerializeField] private OnlineMenu onlineMenu;
        [SerializeField] private LobbyMenuHandler lobbyMenuHandler;
        [SerializeField] private LobbySettingsMenu lobbySettings;
        public CanvasGroup canvasGroup;
        
        private GameObject defaultSelectedUIItem;
        private EventSystem eventSystem;
        private LocalPlayerManager localPlayerManager;

        // Options.
        private int playerCount = 8;
        private int maxPlayersPerClient = 1;
        private byte teamCount = 0;
        private ModIDContentReference _selectedGamemodeContentReference;
        private IGameModeDefinition selectedGamemodeDefinition;
        private GameModeBase selectedGamemode;

        private void Awake()
        {
            localPlayerManager = GameManager.singleton.localPlayerManager;
        }
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            eventSystem = EventSystem.current;
            /*lobbySettings.Open();
            SetupOptions();
            Refresh();*/
            gameObject.SetActive(true);
            if (direction == MenuDirection.BACKWARDS) currentHandler.Back();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            /*
            if(selectedGamemode) Destroy(selectedGamemode.gameObject);
            _selectedGamemodeContentReference = default;
            selectedGamemodeDefinition = null;
            lobbySettings.Close();*/
            gameObject.SetActive(false);
            return true;
        }
        
        /*
        private void Update()
        {
            if (canvasGroup.interactable && UIHelpers.SelectDefaultSelectable(eventSystem, localPlayerManager.systemPlayer))
            {
                eventSystem.SetSelectedGameObject(defaultSelectedUIItem);
            }
        }*/
        
        /*
        public void SetupOptions()
        {
            var backSelectable = lobbySettings.AddOption("Back", "Back");
            backSelectable.onSubmit.AddListener(Button_Back);
            defaultSelectedUIItem = backSelectable.gameObject;
            var playerCountButtons = lobbySettings.AddOption("PlayerCount", "Lobby Size", playerCount);
            playerCountButtons[0].onSubmit.AddListener(DecrementPlayerCount);
            playerCountButtons[1].onSubmit.AddListener(IncrementPlayerCount);
            var playersPerCountButtons = lobbySettings.AddOption("MaxPlayersPerClient", "Players per Client", maxPlayersPerClient);
            playersPerCountButtons[0].onSubmit.AddListener(() => { SetPlayersPerClientCount(maxPlayersPerClient-1); });
            playersPerCountButtons[1].onSubmit.AddListener(() => { SetPlayersPerClientCount(maxPlayersPerClient+1); });
            lobbySettings.AddOption("GameMode",  selectedGamemodeDefinition ? selectedGamemodeDefinition.Name : "None").onSubmit.AddListener(Button_GameMode);
            var teamButtons = lobbySettings.AddOption("Teams", "Teams", teamCount);
            teamButtons[0].onSubmit.AddListener(() => { ChangeTeamCount(-1); });
            teamButtons[1].onSubmit.AddListener(() => { ChangeTeamCount(1); });
            lobbySettings.AddOption("Host", "Host").onSubmit.AddListener(async () => await TryHostLobby());
        }
        
        public void Refresh()
        {
            ((LobbySettingsStringValueContent)lobbySettings.idContentDictionary["GameMode"]).text.text =
                selectedGamemodeDefinition ? selectedGamemodeDefinition.Name : "None";
            if(selectedGamemode) selectedGamemode.AddGamemodeSettings(0, lobbySettings, true);
            lobbySettings.BringOptionToBottom("Teams");
            lobbySettings.BringOptionToBottom("Host");
        }*/

        /*
        private void IncrementPlayerCount()
        {
            playerCount++;
            ((LobbySettingsIntValueContent)lobbySettings.idContentDictionary["PlayerCount"]).text.text 
                = playerCount.ToString();
        }

        private void DecrementPlayerCount()
        {
            if (playerCount == 1) return;
            playerCount--;
            ((LobbySettingsIntValueContent)lobbySettings.idContentDictionary["PlayerCount"]).text.text 
                = playerCount.ToString();
        }

        private void SetPlayersPerClientCount(int value)
        {
            if (value < 1) return;
            maxPlayersPerClient = value;
            ((LobbySettingsIntValueContent)lobbySettings.idContentDictionary["MaxPlayersPerClient"]).text.text =
                maxPlayersPerClient.ToString();
        }

        private void ChangeTeamCount(int change)
        {
            int minTeams = selectedGamemodeDefinition != null ? selectedGamemodeDefinition.minimumTeams : 0;
            int maxTeams = selectedGamemodeDefinition != null ? selectedGamemodeDefinition.maximumTeams : 0;
            teamCount = (byte)Mathf.Clamp(teamCount + change, minTeams, maxTeams);
            Refresh();
        }

        public void Button_Back()
        {
            currentHandler.Back();
        }

        public async void Button_GameMode()
        {
            canvasGroup.interactable = false;
            var csMenu = await ContentSelect.singleton.OpenMenu(0, (int)ContentType.Gamemode, async (a, b) => { await WhenGamemodeSelected(a, b); });
            if (csMenu == null)
            {
                canvasGroup.interactable = true;
                return;
            }
        }

        private async UniTask WhenGamemodeSelected(int player, ModGUIDContentReference arg1)
        {
            canvasGroup.interactable = true;
            ContentSelect.singleton.CloseMenu(0);

            _selectedGamemodeContentReference = arg1;
            if (_selectedGamemodeContentReference.IsValid() == false)
            {
                return;
            }

            IGameModeDefinition gameModeDefinition = ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(_selectedGamemodeContentReference);
            if (gameModeDefinition == null) return;

            if (selectedGamemode)
            {
                selectedGamemode.OnLocalGamemodeSettingsChanged -= Refresh;
                selectedGamemode.ClearGamemodeSettings(0, lobbySettings, true);
                Destroy(selectedGamemode.gameObject);
            }
            selectedGamemodeDefinition = gameModeDefinition;
            await selectedGamemodeDefinition.Load();
            selectedGamemode = GameObject.Instantiate(selectedGamemodeDefinition.GetGamemode(), Vector3.zero, Quaternion.identity).GetComponent<GameModeBase>();
            selectedGamemode.OnLocalGamemodeSettingsChanged += Refresh;
            Refresh();
        }

        public async UniTask TryHostLobby()
        {
            GameManager.singleton.loadingMenu.OpenMenu(0, "Attempting host...");
            int sessionHandlerID = await GameManager.singleton.HostGamemodeSession("", playerCount, "");
            GameManager.singleton.loadingMenu.CloseMenu(0);
            if (sessionHandlerID == -1) return;

            FusionLauncher fl = GameManager.singleton.networkManager.GetSessionHandler(sessionHandlerID);

            await UniTask.WaitUntil(() => fl.sessionManager != null);
            
            SessionManagerGamemode smc = (SessionManagerGamemode)fl.sessionManager;
            
            bool setGamemodeResult = await smc.TrySetGamemode(_selectedGamemodeContentReference);

            smc.SetTeamCount(teamCount);
            smc.SetMaxPlayersPerClient(maxPlayersPerClient);
            smc.CurrentGameMode.SetGamemodeSettings(selectedGamemode);

            lobbyMenuHandler.sessionManagerGamemode = smc;
            
            currentHandler.Forward((int)MainMenuType.LOBBY);
        }*/
    }
}