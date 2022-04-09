using UnityEngine;

namespace rwby.ui.mainmenu
{
    public class OnlineMenu : MainMenuMenu
    {
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
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