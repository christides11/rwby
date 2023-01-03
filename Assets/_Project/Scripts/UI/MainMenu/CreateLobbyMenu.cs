using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion.Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace rwby.ui.mainmenu
{
    public class CreateLobbyMenu : MenuBase
    {
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private LobbyMenuHandler lobbyMenuHandler;
        [SerializeField] private LobbySettingsMenu lobbySettings;
        public CanvasGroup[] canvasGroups;
        
        private EventSystem eventSystem;
        private LocalPlayerManager localPlayerManager;
        
        // Options.
        private int region = 0;
        private string lobbyName = "";
        private string lobbyPassword = "";
        private int playerCount = 8;
        private int maxPlayersPerClient = 1;
        private ModGUIDContentReference _selectedGamemodeContentReference;
        private IGameModeDefinition selectedGamemodeDefinition;
        private GameModeBase selectedGamemode;

        public TextMeshProUGUI menuLabel;
        public TextMeshProUGUI menuDescription;

        private GameObject defaultSelectedUIItem;
        
        private void Awake()
        {
            localPlayerManager = GameManager.singleton.localPlayerManager;
        }
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            eventSystem = EventSystem.current;
            lobbySettings.ClearOptions();
            SetupOptions();
            Refresh();
            gameObject.SetActive(true);
            menuLabel.text = "CREATE LOBBY";
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            if(selectedGamemode) Destroy(selectedGamemode.gameObject);
            _selectedGamemodeContentReference = default;
            selectedGamemodeDefinition = null;
            lobbySettings.ClearOptions();
            gameObject.SetActive(false);
            return true;
        }
        
        private void Update()
        {
            if (canvasGroups[0].interactable && UIHelpers.SelectDefaultSelectable(eventSystem, localPlayerManager.localPlayers[0]) 
                && defaultSelectedUIItem != null)
            {
                eventSystem.SetSelectedGameObject(defaultSelectedUIItem);
            }
        }

        public void SetupOptions()
        {
            var regionOptionSlider = lobbySettings.AddOptionSlider("region", "Region", new []{ "United States", "South Korea", "South America", "Europe", "Japan", "Asia" }, 0);
            regionOptionSlider.OnValueChanged += UpdateRegion;
            defaultSelectedUIItem = regionOptionSlider.gameObject;
            var lobbyNameInputField = lobbySettings.AddInputField("LobbyName", "Lobby Name", $"Lobby {Random.Range(1, 1000)}");
            lobbyNameInputField.inputField.onValueChanged.AddListener(UpdateLobbyName);
            var lobbyPasswordInputField = lobbySettings.AddInputField("Password", "Password", $"");
            lobbyPasswordInputField.inputField.onValueChanged.AddListener(UpdateLobbyPassword);
            var playerCountButtons = lobbySettings.AddIntValueOption("PlayerCount", "Lobby Size", playerCount);
            playerCountButtons.subtractButton.onSubmit.AddListener(DecrementPlayerCount);
            playerCountButtons.addButton.onSubmit.AddListener(IncrementPlayerCount);
            //var playersPerCountButtons = lobbySettings.AddIntValueOption("MaxPlayersPerClient", "Players per Client", maxPlayersPerClient);
            //playersPerCountButtons.subtractButton.onSubmit.AddListener(() => { SetPlayersPerClientCount(maxPlayersPerClient-1); });
            //playersPerCountButtons.addButton.onSubmit.AddListener(() => { SetPlayersPerClientCount(maxPlayersPerClient+1); });
            var gamemodeButtons = lobbySettings.AddStringValueOption("GameMode", "GameMode",
                selectedGamemodeDefinition ? selectedGamemodeDefinition.Name : "None");
            gamemodeButtons.onSubmit.AddListener(Button_GameMode);
            //var teamButtons = lobbySettings.AddIntValueOption("Teams", "Teams", teamCount);
            //teamButtons.subtractButton.onSubmit.AddListener(() => { ChangeTeamCount(-1); });
            //teamButtons.addButton.onSubmit.AddListener(() => { ChangeTeamCount(1); });
            
            lobbySettings.AddOption("Host", "Host").onSubmit.AddListener(async () => await TryHostLobby());
        }

        private void UpdateLobbyPassword(string arg0)
        {
            lobbyPassword = arg0;
        }

        private void UpdateRegion(int value)
        {
            region = value;
        }

        private void UpdateLobbyName(string arg0)
        {
            lobbyName = arg0;
        }

        private void Refresh()
        {
            ((ContentButtonStringValue)lobbySettings.idContentDictionary["GameMode"]).valueString.text =
                selectedGamemodeDefinition ? selectedGamemodeDefinition.Name : "None";
            if(selectedGamemode) selectedGamemode.LobbyUIHandler.AddGamemodeSettings(0, lobbySettings, true);
            lobbySettings.BringOptionToBottom("Teams");
            lobbySettings.BringOptionToBottom("Host");
        }
        
        private void IncrementPlayerCount()
        {
            playerCount++;
            ((ContentButtonIntValue)lobbySettings.idContentDictionary["PlayerCount"]).intValueText.text
                = playerCount.ToString();
        }

        private void DecrementPlayerCount()
        {
            if (playerCount == 1) return;
            playerCount--;
            ((ContentButtonIntValue)lobbySettings.idContentDictionary["PlayerCount"]).intValueText.text
                = playerCount.ToString();
        }

        private void SetPlayersPerClientCount(int value)
        {
            if (value < 1 || value > 4) return;
            maxPlayersPerClient = value;
            ((ContentButtonIntValue)lobbySettings.idContentDictionary["MaxPlayersPerClient"]).intValueText.text =
                maxPlayersPerClient.ToString();
        }

        public async void Button_GameMode()
        {
            foreach (var cg in canvasGroups)
            {
                cg.interactable = false;
            }
            var csMenu = await ContentSelect.singleton.OpenMenu(0, (int)ContentType.Gamemode, async (a, b) => { await WhenGamemodeSelected(a, b); });
            if (csMenu == null)
            {
                foreach (var cg in canvasGroups)
                {
                    cg.interactable = true;
                }
                return;
            }
        }
        
        private async UniTask WhenGamemodeSelected(int player, ModGUIDContentReference arg1)
        {
            foreach (var cg in canvasGroups)
            {
                cg.interactable = true;
            }
            ContentSelect.singleton.CloseMenu(0);

            _selectedGamemodeContentReference = arg1;
            if (_selectedGamemodeContentReference.IsValid() == false)
            {
                return;
            }

            bool loadResult = await ContentManager.singleton.LoadContentDefinition(arg1);
            if (!loadResult) return;
            
            IGameModeDefinition gameModeDefinition = ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(_selectedGamemodeContentReference);
            if (gameModeDefinition == null) return;

            if (selectedGamemode)
            {
                selectedGamemode.OnLocalGamemodeSettingsChanged -= Refresh;
                selectedGamemode.LobbyUIHandler.ClearGamemodeSettings(0, lobbySettings, true);
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
            var appSettings = PhotonAppSettings.Instance.AppSettings;
            appSettings.FixedRegion = NetworkManager.regionCodes[region];
            
            GameManager.singleton.loadingMenu.OpenMenu(0, "Attempting host...");
            int sessionHandlerID = await GameManager.singleton.HostGamemodeSession(lobbyName, playerCount, lobbyPassword);
            GameManager.singleton.loadingMenu.CloseMenu(0);
            if (sessionHandlerID == -1) return;

            FusionLauncher fl = GameManager.singleton.networkManager.GetSessionHandler(sessionHandlerID);

            await UniTask.WaitUntil(() => fl.sessionManager != null);
            
            SessionManagerGamemode smc = (SessionManagerGamemode)fl.sessionManager;
            
            bool setGamemodeResult = await smc.TrySetGamemode(_selectedGamemodeContentReference);

            // TODO
            //smc.SetTeamCount(teamCount);
            smc.SetMaxPlayersPerClient(maxPlayersPerClient);
            smc.CurrentGameMode.SetGamemodeSettings(selectedGamemode);

            lobbyMenuHandler.sessionManagerGamemode = smc;
            
            Debug.Log($"{fl._runner.SessionInfo.Name} ({fl._runner.SessionInfo.Region})");
            GUIUtility.systemCopyBuffer = fl._runner.SessionInfo.Name;

            mainMenu.currentHandler.Forward((int)MainMenuType.LOBBY);
        }
    }
}