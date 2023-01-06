using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.ui.mainmenu
{
    public class ModdingMenu : MenuBase
    {
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            ModIOBrowser.Browser.OpenBrowser(OnBrowserClosedAction);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }
        
        private void OnBrowserClosedAction()
        {
            
        }
    }
}