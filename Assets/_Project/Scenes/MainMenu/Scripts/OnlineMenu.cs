using System;
using System.Collections;
using System.Collections.Generic;
using rwby.menus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rwby.menus
{
    public class OnlineMenu : MonoBehaviour
    {
        [Header("Menus")] 
        public ModeSelectMenu modeSelectMenu;
        public FindLobbyMenu findLobbyMenu;

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void BUTTON_FindLobby()
        {
            //findLobbyMenu.OpenMenu();
            Close();
        }

        public void BUTTON_HostLobby()
        {
            Close();
        }

        public void BUTTON_QuickJoin()
        {
            Close();
        }
        
        public void BUTTON_Back()
        {
            modeSelectMenu.Open();
            Close();
        }
    }
}