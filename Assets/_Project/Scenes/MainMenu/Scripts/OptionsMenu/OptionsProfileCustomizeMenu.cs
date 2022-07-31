using System.Collections;
using System.Collections.Generic;
using rwby.ui.mainmenu;
using UnityEngine;

namespace rwby.ui.mainmenu
{
    public class OptionsProfileCustomizeMenu : MainMenuMenu, IMenuHandler
    {
        public GameObject generalTab;
        public GameObject keyboardTab;
        public GameObject controllerTab;
        
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

        public bool Forward(int menu, bool autoClose = true)
        {
            throw new System.NotImplementedException();
        }

        public bool Back()
        {
            throw new System.NotImplementedException();
        }

        public IList GetHistory()
        {
            throw new System.NotImplementedException();
        }

        public IMenu GetCurrentMenu()
        {
            throw new System.NotImplementedException();
        }

        public void BUTTON_Back()
        {
            currentHandler.Back();
        }

        public void BUTTON_General()
        {
            generalTab.SetActive(true);
            keyboardTab.SetActive(false);
            controllerTab.SetActive(false);
        }

        public void BUTTON_Controller()
        {
            generalTab.SetActive(false);
            keyboardTab.SetActive(false);
            controllerTab.SetActive(true);
        }

        public void BUTTON_Keyboard()
        {
            generalTab.SetActive(false);
            keyboardTab.SetActive(true);
            controllerTab.SetActive(false);
        }
    }
}