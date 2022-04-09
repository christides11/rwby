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
        [SerializeField] private ModeSelectMenu modeSelect;
        [SerializeField] private LocalMenu local;
        [SerializeField] private OnlineMenu online;
        [SerializeField] private OptionsMenu options;
        [SerializeField] private HostLobbyMenu hostLobby;
        [SerializeField] private LobbyMenuHandler lobbyMenu;
        
        private void Awake()
        {
            menus.Add((int)MainMenuType.TITLE_SCREEN, titleScreen);
            menus.Add((int)MainMenuType.MODE_SELECT, modeSelect);
            menus.Add((int)MainMenuType.LOCAL, local);
            menus.Add((int)MainMenuType.ONLINE, online);
            menus.Add((int)MainMenuType.OPTIONS, options);
            menus.Add((int)MainMenuType.HOST_LOBBY, hostLobby);
            menus.Add((int)MainMenuType.LOBBY, lobbyMenu);
            history.Add((int)startingMenu);

            foreach (var menu in menus.Values)
            {
                menu.TryClose(MenuDirection.BACKWARDS, true);
            }
            menus[(int)startingMenu].Open(MenuDirection.FORWARDS, this);
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

        public IMenu GetPreviousMenu()
        {
            if (history.Count == 0) return null;
            return menus[history[history.Count - 1]];
        }
        
        public IMenu GetCurrentMenu()
        {
            if (history.Count == 0) return null;
            return menus[history[history.Count-1]];
        }
    }
}