using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby.ui.mainmenu
{
    public class MainMenuManager : MonoBehaviour, IMenuHandler
    {
        public Dictionary<int, MainMenuMenu> menus = new Dictionary<int, MainMenuMenu>();

        [SerializeField] private List<int> history = new List<int>();
        public MainMenuType startingMenu = MainMenuType.TITLE_SCREEN;
        
        [SerializeField] private TitleScreenMenu titleScreen;
        [SerializeField] private MainMenu main;
        //[SerializeField] private LocalMenu local;
        //[SerializeField] private OnlineMenu online;
        //[SerializeField] private OptionsMenu options;
        //[SerializeField] private HostLobbyMenu hostLobby;
        [SerializeField] private LobbyMenuHandler lobbyMenu;
        //[SerializeField] private FindLobbyMenu findLobby;
        
        
        private void Awake()
        {
            menus.Add((int)MainMenuType.TITLE_SCREEN, titleScreen);
            menus.Add((int)MainMenuType.MAIN_MENU, main);
            menus.Add((int)MainMenuType.LOBBY, lobbyMenu);
            history.Add((int)startingMenu);

            foreach (var menu in menus.Values)
            {
                menu.TryClose(MenuDirection.BACKWARDS, true);
            }
            menus[(int)startingMenu].Open(MenuDirection.FORWARDS, this);
        }

        private void Start()
        {
            var networkManager = GameManager.singleton.networkManager;
            if (networkManager.sessions.ContainsKey(networkManager.mainSession))
            {
                menus[(int)MainMenuType.TITLE_SCREEN].TryClose(MenuDirection.FORWARDS, true);
                lobbyMenu.sessionManagerGamemode = (SessionManagerGamemode)networkManager.GetSessionHandler(networkManager.mainSession).sessionManager;
                Forward((int)MainMenuType.MAIN_MENU);
                Forward((int)MainMenuType.LOBBY);
            }else if (BootLoader.bootLoaded)
            {
                //menus[(int)MainMenuType.TITLE_SCREEN].TryClose(MenuDirection.FORWARDS, true);
                //Forward((int)MainMenuType.MAIN_MENU);
            }
            
        }

        public bool Forward(int menu, bool autoClose = true)
        {
            if (!menus.ContainsKey(menu)) return false;
            EventSystem.current.SetSelectedGameObject(null);
            if (autoClose) GetCurrentMenu().TryClose(MenuDirection.FORWARDS, false);
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

        public IMenu GetPreviousMenu()
        {
            if (history.Count == 0) return null;
            return menus[history[^1]];
        }
        
        public IMenu GetCurrentMenu()
        {
            if (history.Count == 0) return null;
            return menus[history[^1]];
        }
    }
}