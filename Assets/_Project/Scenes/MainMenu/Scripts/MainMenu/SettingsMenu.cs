using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.ui.mainmenu
{
    public class SettingsMenu : MenuBase
    {
        public rwby.ui.SettingsMenu settingsMenu;

        private void Awake()
        {
            
        }

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            settingsMenu.Open(direction, menuHandler);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            settingsMenu.TryClose(direction);
            gameObject.SetActive(false);
            return true;
        }
    }
}