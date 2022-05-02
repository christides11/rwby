using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby.ui.mainmenu
{
    public class OptionsMenu : MainMenuMenu, IMenuHandler
    {
        public enum OptionsSubmenuType
        {
            GENERAL,
            PROFILES
        }
        public Dictionary<int, MenuBase> menus = new Dictionary<int, MenuBase>();
        [SerializeField] private List<int> history = new List<int>();
        
        [SerializeField] private OptionsGeneralMenu generalMenu;
        [SerializeField] private OptionsProfilesMenu profiles;
        [SerializeField] private GameObject defaultSelectedUIItem;
        
        private LocalPlayerManager localPlayerManager;
        
        private void Start()
        {
            localPlayerManager = GameManager.singleton.localPlayerManager;
        }

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            menus.Clear();
            history.Clear();
            menus.Add((int)OptionsSubmenuType.GENERAL, generalMenu);
            menus.Add((int)OptionsSubmenuType.PROFILES, profiles);
            generalMenu.Open(MenuDirection.FORWARDS, this);
            history.Add((int)OptionsSubmenuType.GENERAL);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            Back();
            if (history.Count > 0) return false;
            gameObject.SetActive(false);
            history.Clear();
            return true;
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
            if (history.Count == 0) return false;
            bool result = GetCurrentMenu().TryClose(MenuDirection.BACKWARDS);
            if(result) history.RemoveAt(history.Count-1);
            GetCurrentMenu().Open(MenuDirection.BACKWARDS, this);
            return true;
        }

        public IList GetHistory()
        {
            return history;
        }

        public IMenu GetCurrentMenu()
        {
            if (history.Count == 0) return null;
            return menus[history[history.Count-1]];
        }

        private void Update()
        {
            if (UIHelpers.SelectDefaultSelectable(EventSystem.current, localPlayerManager.systemPlayer))
            {
                EventSystem.current.SetSelectedGameObject(defaultSelectedUIItem);
            }
        }
    }
}