using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;
using Cysharp.Threading.Tasks;

namespace rwby.ui.mainmenu
{
    // TODO: Unload temporary gamemode when going backwards.
    public class HostLobbyMenu : MainMenuMenu
    {
        [SerializeField] private OnlineMenu onlineMenu;
        [SerializeField] private LobbyMenuHandler lobbyMenuHandler;
        [SerializeField] private LobbySettingsMenu lobbySettings;

        public TMP_InputField lobbyNameTextMesh;
        public TMP_Dropdown playerCountDropdown;
        public Toggle privateLobbyToggle;

        // Options.
        private int playerCount = 8;
        private int maxPlayersPerClient = 4;
        private byte teamCount = 0;
        private ModObjectReference selectedGamemodeReference;
        private IGameModeDefinition selectedGamemodeDefinition;
        private GameModeBase selectedGamemode;

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            lobbySettings.Open();
            Refresh();
            gameObject.SetActive(true);
            if (direction == MenuDirection.BACKWARDS) currentHandler.Back();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            if(selectedGamemode) Destroy(selectedGamemode.gameObject);
            selectedGamemodeReference = default;
            selectedGamemodeDefinition = null;
            lobbySettings.Close();
            gameObject.SetActive(false);
            return true;
        }

        public void Refresh()
        {
            lobbySettings.ClearOptions();
            lobbySettings.AddOption("Back").onSubmit.AddListener(Button_Back);
            var playerCountButtons = lobbySettings.AddOption("Player Count", playerCount);
            playerCountButtons[0].onSubmit.AddListener(DecrementPlayerCount);
            playerCountButtons[1].onSubmit.AddListener(IncrementPlayerCount);
            var playersPerCountButtons = lobbySettings.AddOption("Max Players per Client", maxPlayersPerClient);
            playersPerCountButtons[0].onSubmit.AddListener(() => { maxPlayersPerClient--; });
            playersPerCountButtons[1].onSubmit.AddListener(() => { maxPlayersPerClient++; });
            lobbySettings.AddOption("GameMode",  selectedGamemodeDefinition ? selectedGamemodeDefinition.Name : "None").onSubmit.AddListener(Button_GameMode);
            if(selectedGamemode) selectedGamemode.AddGamemodeSettings(0, lobbySettings, true);
            var teamButtons = lobbySettings.AddOption("Teams", teamCount);
            teamButtons[0].onSubmit.AddListener(() => { ChangeTeamCount(-1); });
            teamButtons[1].onSubmit.AddListener(() => { ChangeTeamCount(1); });
            lobbySettings.AddOption("Host").onSubmit.AddListener(async () => await TryHostLobby());
        }

        private void IncrementPlayerCount()
        {
            playerCount++;
            Refresh();
        }

        private void DecrementPlayerCount()
        {
            if (playerCount == 1) return;
            playerCount--;
            Refresh();
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

        public void Button_GameMode()
        {
            ContentSelect.singleton.OpenMenu<IGameModeDefinition>(0, async (a, b) => { await WhenGamemodeSelected(a, b); });
        }

        private async UniTask WhenGamemodeSelected(int player, ModObjectReference arg1)
        {
            Debug.Log($"Gamemode {arg1} selected.");
            ContentSelect.singleton.CloseMenu(0);

            selectedGamemodeReference = arg1;
            if (selectedGamemodeReference.IsValid() == false)
            {
                return;
            }

            IGameModeDefinition gameModeDefinition = ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(selectedGamemodeReference);
            if (gameModeDefinition == null) return;

            if (selectedGamemode)
            {
                selectedGamemode.OnLocalGamemodeSettingsChanged -= Refresh;
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
            int sessionHandlerID = await GameManager.singleton.HostGamemodeSession(lobbyNameTextMesh.text, playerCount, false);
            GameManager.singleton.loadingMenu.CloseMenu(0);
            if (sessionHandlerID == -1) return;

            FusionLauncher fl = GameManager.singleton.networkManager.GetSessionHandler(sessionHandlerID);

            await UniTask.WaitUntil(() => fl.sessionManager != null);
            
            SessionManagerGamemode smc = (SessionManagerGamemode)fl.sessionManager;
            
            bool setGamemodeResult = await smc.TrySetGamemode(selectedGamemodeReference);

            smc.SetTeamCount(teamCount);
            smc.SetMaxPlayersPerClient(maxPlayersPerClient);
            smc.CurrentGameMode.SetGamemodeSettings(selectedGamemode);

            lobbyMenuHandler.sessionManagerGamemode = smc;
            
            currentHandler.Forward((int)MainMenuType.LOBBY);
        }
    }
}