using System.Collections;
using System.Collections.Generic;
using rwby.ui.mainmenu;
using UnityEngine;

namespace rwby.ui
{
    public class SettingsKeyboardMenu : MenuBase
    {
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            return base.TryClose(direction, forceClose);
        }
    }
}