using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby.ui.mainmenu
{
    public class OnlineMenu : MainMenuMenu
    {
        public GameObject defaultSelectedUIItem;
        private EventSystem eventSystem;
        private LocalPlayerManager localPlayerManager;
        
        private void Awake()
        {
            localPlayerManager = GameManager.singleton.localPlayerManager;
        }
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            eventSystem = EventSystem.current;
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }

        private void Update()
        {
            if (UIHelpers.SelectDefaultSelectable(eventSystem, localPlayerManager.systemPlayer))
            {
                eventSystem.SetSelectedGameObject(defaultSelectedUIItem);
            }
        }

        public void BUTTON_FindLobby()
        {
            currentHandler.Forward((int)MainMenuType.FIND_LOBBY);
        }

        public void BUTTON_HostLobby()
        {
            currentHandler.Forward((int)MainMenuType.HOST_LOBBY);
        }

        public void BUTTON_QuickJoin()
        {
            
        }
        
        public void BUTTON_Back()
        {
            currentHandler.Back();
        }
    }
}