using rwby.ui.mainmenu;

namespace rwby
{
    public interface IMenu
    {
        public void Open(MenuDirection direction, IMenuHandler menuHandler);
        public bool TryClose(MenuDirection direction, bool forceClose = false);
    }
}