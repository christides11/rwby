using System.Collections;
using System.Collections.Generic;
using rwby.ui;
using UnityEngine;

namespace rwby.core.training {
    public class TrainingSettingsMenu : MenuBase
    {
        public GamemodeTraining gamemodeTraining;

        public GameObject optionsMenuObject;
        public CPUSettingsMenu cpuSettingsMenu;

        public IClosableMenu currentSubmenu;

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            optionsMenuObject.SetActive(true);
            cpuSettingsMenu.gameObject.SetActive(false);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            if(currentSubmenu != null)
            {
                bool result = currentSubmenu.TryClose();
                if (result)
                {
                    currentSubmenu = null;
                }
                return false;
            }
            gameObject.SetActive(false);
            return true;
        }
        /*
        public void Open()
        {
            gameObject.SetActive(true);
            //PlayerPointerHandler.singleton.ShowMice();

            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            optionsMenuObject.SetActive(true);
            cpuSettingsMenu.gameObject.SetActive(false);
        }

        public bool TryClose()
        {
            if(currentSubmenu != null)
            {
                bool result = currentSubmenu.TryClose();
                if (result)
                {
                    currentSubmenu = null;
                }
                //PlayerPointerHandler.singleton.ShowMice();
                return false;
            }
            gameObject.SetActive(false);
            //PlayerPointerHandler.singleton.HideMice();
            return true;
        }*/

        public void OpenMenu_CPUSettings()
        {
            currentSubmenu = cpuSettingsMenu;
            cpuSettingsMenu.Open();
        }
    }
}