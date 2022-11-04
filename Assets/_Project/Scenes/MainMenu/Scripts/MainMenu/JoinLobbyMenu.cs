using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion.Photon.Realtime;
using TMPro;
using UnityEngine;

namespace rwby.ui.mainmenu
{
    public class JoinLobbyMenu : MenuBase
    {
        public TextMeshProUGUI menuLabel;
        public TextMeshProUGUI menuDescription;

        public OptionSlider regionSlider;
        public ContentButtonInputField lobbyCodeField;
        public ContentButtonInputField passwordField;

        [Header("Menus")]
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private LobbyMenuHandler lobbyMenuHandler;

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            menuLabel.text = "JOIN LOBBY";
            menuDescription.text = "";
            regionSlider.options = new[] { "United States", "South Korea", "South America", "Europe", "Japan", "Asia" };
            regionSlider.SetOption(0);
        }
        
        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            var appSettings = PhotonAppSettings.Instance.AppSettings;
            appSettings.FixedRegion = "";
            gameObject.SetActive(false);
            return true;
        }

        public void BUTTON_Join()
        {
            _ = TryJoinLobby();
        }

        async UniTask TryJoinLobby()
        {
            var appSettings = PhotonAppSettings.Instance.AppSettings;
            appSettings.FixedRegion = NetworkManager.regionCodes[regionSlider.currentOption];
            var joinResult = await GameManager.singleton.networkManager.TryJoinSession(lobbyCodeField.inputField.text, passwordField.inputField.text);

            if (!joinResult.Item1)
            {
                return;
            }

            var sessionHandler = GameManager.singleton.networkManager.GetSessionHandler(joinResult.Item2);

            lobbyMenuHandler.sessionManagerGamemode = (SessionManagerGamemode)sessionHandler.sessionManager;
            
            mainMenu.currentHandler.Forward((int)MainMenuType.LOBBY);
        }
    }
}