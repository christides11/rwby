using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.ui.mainmenu
{
    public class OptionsGeneralMenu : MenuBase
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

        public void BUTTON_Profiles()
        {
            currentHandler.Forward((int)OptionsMenu.OptionsSubmenuType.PROFILE_SELECTION);
        }
        
        public void BUTTON_KeyboardControls()
        {
            
        }

        public void BUTTON_Audio()
        {
            
        }

        public void BUTTON_Video()
        {
            
        }

        public void BUTTON_Accessibility()
        {
            
        }

        public void BUTTON_Language()
        {
            
        }

        public void BUTTON_Credits()
        {
            
        }

        public void BUTTON_Back()
        {
            currentHandler.Back();
        }
    }
}