using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby.ui.mainmenu
{
    public class LocalMenu : MainMenuMenu
    {
        public GameObject defaultSelectedUIItem;
        private LocalPlayerManager localPlayerManager;

        private void Awake()
        {
            localPlayerManager = GameManager.singleton.localPlayerManager;
        }
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            UIHelpers.TrySelectDefault(EventSystem.current, defaultSelectedUIItem, localPlayerManager.systemPlayer);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
            return true;
        }
        
        private void Update()
        {
            if (UIHelpers.SelectDefaultSelectable(EventSystem.current, localPlayerManager.systemPlayer))
            {
                EventSystem.current.SetSelectedGameObject(defaultSelectedUIItem);
            }
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