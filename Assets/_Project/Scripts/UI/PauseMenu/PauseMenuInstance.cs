using System;
using System.Collections;
using System.Collections.Generic;
using rwby.ui.mainmenu;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby.ui
{
    public class PauseMenuInstance : MonoBehaviour, IMenuHandler
    {
        public enum PauseMenus
        {
            MAIN,
            SETTINGS
        }
        public Dictionary<int, MenuBase> menus = new Dictionary<int, MenuBase>();

        [SerializeField] private List<int> history = new List<int>();

        public int playerID = 0;
        public PauseMenu pauseMenuHandler;

        [Header("Menus")] 
        public PauseMainMenu mainMenu;
        public PauseSettingsMenu settingsMenu;
        
        private void Awake()
        {
            menus.Add((int)PauseMenus.MAIN, mainMenu);
            menus.Add((int)PauseMenus.SETTINGS, settingsMenu);
        }

        public void Open()
        {
            Forward((int)PauseMenus.MAIN);
        }

        public bool Close(bool forceClose = false)
        {
            Back();
            if (history.Count > 1) return false;
            return true;
        }
        
        public bool Forward(int menu, bool autoClose = true)
        {
            if (!menus.ContainsKey(menu)) return false;
            EventSystem.current.SetSelectedGameObject(null);
            if (autoClose && history.Count != 0) GetCurrentMenu().TryClose(MenuDirection.FORWARDS, true);
            menus[menu].Open(MenuDirection.FORWARDS, this);
            history.Add(menu);
            return true;
        }

        public bool Back()
        {
            if (history.Count <= 1) return false;
            bool result = GetCurrentMenu().TryClose(MenuDirection.BACKWARDS);
            if(result == true) history.RemoveAt(history.Count-1);
            GetCurrentMenu().Open(MenuDirection.BACKWARDS, this);
            return result;
        }

        public IList GetHistory()
        {
            return history;
        }

        public IMenu GetCurrentMenu()
        {
            if (history.Count == 0) return null;
            return menus[history[^1]];
        }
    }
}