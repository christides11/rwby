using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.core.training {
    public class TrainingSettingsMenu : MonoBehaviour, IClosableMenu
    {
        public GamemodeTraining gamemodeTraining;

        public GameObject optionsMenuObject;
        public CPUSettingsMenu cpuSettingsMenu;

        public IClosableMenu currentSubmenu;

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
        }

        public void OpenMenu_CPUSettings()
        {
            currentSubmenu = cpuSettingsMenu;
            cpuSettingsMenu.Open();
        }
    }
}