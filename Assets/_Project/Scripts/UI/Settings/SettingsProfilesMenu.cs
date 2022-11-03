using System;
using System.Collections;
using System.Collections.Generic;
using rwby.ui.mainmenu;
using UnityEngine;
using UnityEngine.Events;

namespace rwby.ui
{
    public class SettingsProfilesMenu : MenuBase
    {
        public GameObject profilesListMenu;
        public GameObject rebindMenu;

        public Transform profilesContentHolder;
        public ContentButtonBase profilePrefab;
        
        public SettingsMenu settingsMenu;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            profilesListMenu.SetActive(true);
            gameObject.SetActive(true);

            RefreshProfilesList();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }
        
        private void RefreshProfilesList()
        {
            foreach (Transform child in profilesContentHolder)
            {
                Destroy(child.gameObject);
            }

            for(int i = 0; i < GameManager.singleton.profilesManager.Profiles.Count; i++)
            {
                var profile = GameManager.singleton.profilesManager.Profiles[i];
                var profileIndex = i;
                var p = GameObject.Instantiate(profilePrefab, profilesContentHolder, false);
                p.label.text = profile.profileName;
                p.onSubmit.AddListener(() => OnProfileSelected(profileIndex));
            }
        }

        UnityAction screenClosedEvent;
        private void OnProfileSelected(int profileIndex)
        {
            int index = profileIndex;
            var profilesManager = GameManager.singleton.profilesManager;
            Debug.Log(profilesManager.Profiles[profileIndex]);
            profilesManager.ApplyProfileToPlayer(settingsMenu.playerID, profileIndex, false);
            screenClosedEvent = () => ApplyProfileChanges(index);
            GameManager.singleton.cMapper.onScreenClosed += screenClosedEvent;
            GameManager.singleton.cMapper.Open();
        }

        private void ApplyProfileChanges(int profileIndex)
        {
            GameManager.singleton.profilesManager.ApplyControlsToProfile(settingsMenu.playerID, profileIndex, false);
            GameManager.singleton.profilesManager.SaveProfiles();
            GameManager.singleton.cMapper.onScreenClosed -= screenClosedEvent;
        }
    }
}