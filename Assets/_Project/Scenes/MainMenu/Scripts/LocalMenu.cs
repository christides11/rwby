using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby.menus
{
    public class LocalMenu : MonoBehaviour
    {
        [Header("Menus")] 
        public ModeSelectMenu modeSelectMenu;
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
        }

        public void BUTTON_PlayerMatch()
        {
            
        }

        public void BUTTON_Training()
        {
            
        }

        public void BUTTON_Tutorial()
        {
            
        }

        public void BUTTON_Back()
        {
            modeSelectMenu.Open();
            Close();
        }
    }
}