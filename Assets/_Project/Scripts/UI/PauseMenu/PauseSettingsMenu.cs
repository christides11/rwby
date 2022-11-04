using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.ui
{
    public class PauseSettingsMenu : MenuBase
    {
        public PauseMenuInstance pauseMenuInstance;

        public rwby.ui.SettingsMenu settingsMenu;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            settingsMenu.playerID = pauseMenuInstance.playerID;
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }
    }
}