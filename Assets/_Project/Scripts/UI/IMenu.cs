using rwby.ui.mainmenu;

namespace rwby.ui
{
    public interface IMenu
    {
        public void Open(MenuDirection direction, IMenuHandler menuHandler);
        public bool TryClose(MenuDirection direction, bool forceClose = false);
    }
}