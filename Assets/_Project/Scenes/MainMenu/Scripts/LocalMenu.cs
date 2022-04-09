namespace rwby.ui.mainmenu
{
    public class LocalMenu : MainMenuMenu
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

        public void BUTTON_PlayerMatch()
        {
            
        }

        public void BUTTON_Training()
        {
            
        }

        public void BUTTON_Tutorial()
        {
            
        }

        public void BUTTON_Back()
        {
            currentHandler.Back();
        }
    }
}