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
        public ModeSelectMenu modeSelectMenu;

        [Header("Buttons")] 
        public Button buttonFindLobby;
        public Button buttonHostLobby;
        public Button buttonQuickJoin;
        public Button buttonBack;

        private void Start()
        {
            buttonFindLobby.GetComponent<EventTrigger>().AddOnSubmitListeners((a) => { BUTTON_FindLobby(); });
            buttonHostLobby.GetComponent<EventTrigger>().AddOnSubmitListeners((a) => { BUTTON_HostLobby(); });
            buttonQuickJoin.GetComponent<EventTrigger>().AddOnSubmitListeners((a) => { BUTTON_QuickJoin(); });
            buttonBack.GetComponent<EventTrigger>().AddOnSubmitListeners((a) => { BUTTON_Back(); });
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            modeSelectMenu.Open();
            gameObject.SetActive(false);
        }

        void BUTTON_FindLobby()
        {
            
        }

        void BUTTON_HostLobby()
        {
            
        }

        void BUTTON_QuickJoin()
        {
            
        }
        
        void BUTTON_Back()
        {
            modeSelectMenu.Open();
            Close();
        }
    }
}