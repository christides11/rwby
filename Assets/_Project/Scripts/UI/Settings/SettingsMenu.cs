using System;
using System.Collections;
using System.Collections.Generic;
using rwby.ui.mainmenu;
using UnityEngine;

namespace rwby.ui
{
    public class SettingsMenu : MenuBase
    {
        public int playerID = -1;
        
        [Header("Menus")] 
        public SettingsProfilesMenu profilesMenu;
        public SettingsKeyboardMenu keyboardMenu;
        public SettingsAudioMenu audioMenu;
        public SettingsVideoMenu videoMenu;
        
        private void Awake()
        {
            
        }

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            profilesMenu.TryClose(MenuDirection.BACKWARDS, true);
            keyboardMenu.TryClose(MenuDirection.BACKWARDS, true);
            audioMenu.TryClose(MenuDirection.BACKWARDS, true);
            videoMenu.TryClose(MenuDirection.BACKWARDS, true);
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            if (!CloseAllMenus()) return false;
            gameObject.SetActive(false);
            playerID = -1;
            return true;
        }

        public bool CloseAllMenus()
        {
            if (!profilesMenu.TryClose(MenuDirection.BACKWARDS, true)) return false;
            if (!keyboardMenu.TryClose(MenuDirection.BACKWARDS, true)) return false;
            if (!audioMenu.TryClose(MenuDirection.BACKWARDS, true)) return false;
            if (!videoMenu.TryClose(MenuDirection.BACKWARDS, true)) return false;
            return true;
        }
        
        public void Open_ProfilesMenu()
        {
            if (!CloseAllMenus()) return;
            profilesMenu.Open(MenuDirection.FORWARDS, null);
        }
        
        public void Open_KeyboardMenu()
        {
            if (!CloseAllMenus()) return;
            keyboardMenu.Open(MenuDirection.FORWARDS, null);
        }
        
        public void Open_AudioMenu()
        {
            if (!CloseAllMenus()) return;
            audioMenu.Open(MenuDirection.FORWARDS, null);
        }

        public void Open_VideoMenu()
        {
            if (!CloseAllMenus()) return;
            videoMenu.Open(MenuDirection.FORWARDS, null);
        }
    }
}