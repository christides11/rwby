using UnityEngine;

namespace rwby.ui
{
    public class MenuBase : MonoBehaviour, IMenu
    {
        public IMenuHandler currentHandler;
        
        public virtual void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            currentHandler = menuHandler;
        }

        public virtual bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            return true;
        }
    }
}