using System;
using System.Collections;
using System.Collections.Generic;
using rwby.ui.mainmenu;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace rwby.ui
{
    public class SettingsProfilesMenu : MenuBase, IMenuHandler
    {
        public enum ProfilesSubMenuTypes
        {
            PROFILE_SELECT,
            PROFILE_CUSTOMIZATION,
            REMAP_CONTROLS
        }

        [HideInInspector] public int currentSelectedProfile = -1;
        
        public Dictionary<int, MenuBase> menus = new Dictionary<int, MenuBase>();
        [SerializeField] private List<int> history = new List<int>();

        public GameObject rebindMenu;

        public Transform profilesContentHolder;
        public ContentButtonBase profilePrefab;
        
        public SettingsMenu settingsMenu;

        [Header("SubMenus")]
        public SettingsProfileSelectMenu profileSelectMenu;
        public SettingsProfileCustomizationMenu profileCustomizationMenu;

        private void Awake()
        {
            menus.Add((int)ProfilesSubMenuTypes.PROFILE_SELECT, profileSelectMenu);
            menus.Add((int)ProfilesSubMenuTypes.PROFILE_CUSTOMIZATION, profileCustomizationMenu);
            profileSelectMenu.gameObject.SetActive(false);
            profileCustomizationMenu.gameObject.SetActive(false);
        }

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            TryCloseAll();
            gameObject.SetActive(true);
            Forward((int)ProfilesSubMenuTypes.PROFILE_SELECT);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            TryCloseAll();
            gameObject.SetActive(false);
            return true;
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