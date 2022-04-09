using UnityEngine;
using Rewired;
using UnityEngine.EventSystems;

namespace rwby.ui.mainmenu
{
    public class ModeSelectMenu : MainMenuMenu
    {
        public LobbyMenuHandler lobbyMenuHandler;
        public GameObject defaultSelectedUIItem;

        private Rewired.Player systemPlayer;
        private EventSystem eventSystem;
        private LocalPlayerManager localPlayerManager;

        private void Start()
        {
            localPlayerManager = GameManager.singleton.localPlayerManager;
        }

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            systemPlayer = ReInput.players.GetSystemPlayer();
            eventSystem = EventSystem.current;
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
            return true;
        }

        private void Update()
        {
            if (eventSystem.currentSelectedGameObject == null
                && localPlayerManager.systemPlayer.controllerType == PlayerControllerType.GAMEPAD
                && systemPlayer.GetAxis2D(rwby.Action.UIMovement_X, rwby.Action.UIMovement_Y).sqrMagnitude > 0)
            {
                eventSystem.SetSelectedGameObject(defaultSelectedUIItem);
            }
        }

        public void BUTTON_Online()
        {
            currentHandler.Forward((int)MainMenuType.ONLINE);
        }

        public void BUTTON_Local()
        {
            currentHandler.Forward((int)MainMenuType.LOCAL);
        }

        public void BUTTON_Modding()
        {
            
        }
        
        public void BUTTON_Options()
        {
            currentHandler.Forward((int)MainMenuType.OPTIONS);
        }

        public void BUTTON_Exit()
        {
            Application.Quit();
        }
    }
}