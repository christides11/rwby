using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using UnityEngine.EventSystems;

namespace rwby.ui.mainmenu
{
    public class MainMenu : MainMenuMenu, IMenuHandler
    {
        public enum MainMenusType
        {
            NULL,
            QUICK_MATCH,
            FIND_LOBBY,
            JOIN_LOBBY,
            CREATE_LOBBY,
            SETTINGS
        }
        public LobbyMenuHandler lobbyMenuHandler;
        public GameObject defaultSelectedUIItem;

        private Rewired.Player systemPlayer;
        private EventSystem eventSystem;
        private LocalPlayerManager localPlayerManager;

        public Dictionary<int, MenuBase> menus = new Dictionary<int, MenuBase>();
        [SerializeField] private List<int> history = new List<int>();
        
        public TextMeshProUGUI menuLabel;
        public TextMeshProUGUI menuDescription;
        
        [Header("Menus")] public QuickMatchMenu quickMatchMenu;
        public FindLobbyMenu findLobbyMenu;
        public JoinLobbyMenu joinLobbyMenu;
        public CreateLobbyMenu createLobbyMenu;
        public rwby.ui.mainmenu.SettingsMenu settingsMenu;

        private void Awake()
        {
            localPlayerManager = GameManager.singleton.localPlayerManager;
            menus.Add((int)MainMenusType.QUICK_MATCH, quickMatchMenu);
            menus.Add((int)MainMenusType.FIND_LOBBY, findLobbyMenu);
            menus.Add((int)MainMenusType.JOIN_LOBBY, joinLobbyMenu);
            menus.Add((int)MainMenusType.CREATE_LOBBY, createLobbyMenu);
            menus.Add((int)MainMenusType.SETTINGS, settingsMenu);
        }

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            systemPlayer = ReInput.players.GetSystemPlayer();
            eventSystem = EventSystem.current;
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            TryCloseAll();
            EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
            return true;
        }

        private void Update()
        {
            if (UIHelpers.SelectDefaultSelectable(eventSystem, localPlayerManager.systemPlayer))
            {
                eventSystem.SetSelectedGameObject(defaultSelectedUIItem);
            }
        }

        public void BUTTON_QuickMatch()
        {
            bool closeResult = TryCloseAll();
            if (!closeResult) return;
            menuLabel.text = "MAIN MENU";
            menuDescription.text = "";
            Forward((int)MainMenusType.QUICK_MATCH);
        }

        public void BUTTON_FindLobby()
        {
            bool closeResult = TryCloseAll();
            if (!closeResult) return;
            menuLabel.text = "MAIN MENU";
            menuDescription.text = "";
            Forward((int)MainMenusType.FIND_LOBBY);
        }
        
        public void BUTTON_JoinLobby()
        {
            bool closeResult = TryCloseAll();
            if (!closeResult) return;
            menuLabel.text = "MAIN MENU";
            menuDescription.text = "";
            Forward((int)MainMenusType.JOIN_LOBBY);
        }
        
        public void BUTTON_CreateLobby()
        {
            bool closeResult = TryCloseAll();
            if (!closeResult) return;
            menuLabel.text = "MAIN MENU";
            menuDescription.text = "";
            Forward((int)MainMenusType.CREATE_LOBBY);
        }
        
        public void BUTTON_Training()
        {
        }
        
        public void BUTTON_Tutorial()
        {
        }
        
        public void BUTTON_LocalMatch()
        {
        }
        
        public void BUTTON_Modding()
        {
            
        }
        
        public void BUTTON_Options()
        {
            bool closeResult = TryCloseAll();
            if (!closeResult) return;
            menuLabel.text = "SETTINGS";
            menuDescription.text = "";
            Forward((int)MainMenusType.SETTINGS);
        }

        public void BUTTON_Exit()
        {
            Application.Quit();
        }

        public bool TryCloseAll()
        {
            while (history.Count > 0)
            {
                bool closeResult = Back();
                if (!closeResult) return false;
            }
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
            if (history.Count <= 0) return false;
            bool menuClosed = GetCurrentMenu().TryClose(MenuDirection.BACKWARDS);
            if (!menuClosed) return false;
            history.RemoveAt(history.Count-1);
            if(history.Count != 0) GetCurrentMenu().Open(MenuDirection.BACKWARDS, this);
            return true;
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