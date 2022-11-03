using System.Collections;
using System.Collections.Generic;
using rwby.ui.mainmenu;
using UnityEngine;

namespace rwby.ui
{
    public class SettingsProfilesMenu : MenuBase
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
    }
}