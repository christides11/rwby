using System;
using System.Collections;
using System.Collections.Generic;
using rwby.ui;
using rwby.ui.mainmenu;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby
{
    public class LobbyMenuInstance : MonoBehaviour, IMenuHandler
    {
        public int playerID;
        
        public Canvas canvas;
        
        public LobbyMenuHandler lobbyMenuHandler;

        [Header("Menus")] 
        [SerializeField] private LobbyMenu lobbyMenu;
        [SerializeField] private CharacterSelectMenu characterSelectMenu;

        [SerializeField] private List<int> history = new List<int>();
        public Dictionary<int, MenuBase> menus = new Dictionary<int, MenuBase>();

        private LocalPlayerManager localPlayerManager;
        private EventSystem eventSystem;
        
        public void Initialize(LobbyMenuHandler menuHandler)
        {
            menus.Add((int)LobbyMenuType.LOBBY, lobbyMenu);
            menus.Add((int)LobbyMenuType.CHARACTER_SELECT, characterSelectMenu);
            history.Add((int)LobbyMenuType.LOBBY);
            
            foreach (var menu in menus.Values)
            {
                menu.TryClose(MenuDirection.BACKWARDS, true);
            }
            menus[(int)LobbyMenuType.LOBBY].Open(MenuDirection.FORWARDS, this);
            
            this.lobbyMenuHandler = menuHandler;
            
            localPlayerManager = GameManager.singleton.localPlayerManager;
            eventSystem = EventSystem.current;
        }

        public void Refresh()
        {
            lobbyMenu.Refresh();
            characterSelectMenu.Refresh();
        }

        public void Cleanup()
        {
            
        }
        
        public bool Forward(int menu, bool autoClose = true)
        {
            if (!menus.ContainsKey(menu)) return false;
            EventSystem.current.SetSelectedGameObject(null);
            if (autoClose) GetCurrentMenu().TryClose(MenuDirection.FORWARDS, true);
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