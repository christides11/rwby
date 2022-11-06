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
            //GameManager.singleton.localPlayerManager.AutoAssignControllers();
            int index = profileIndex;
            var profilesManager = GameManager.singleton.profilesManager;
            profilesManager.ApplyProfileToPlayer(settingsMenu.playerID, profileIndex);
            screenClosedEvent = () => ApplyProfileChanges(index);
            GameManager.singleton.cMapper.onScreenClosed += screenClosedEvent;
            GameManager.singleton.cMapper.Open();
        }

        private void ApplyProfileChanges(int profileIndex)
        {
            var playerID = settingsMenu.playerID;
            GameManager.singleton.profilesManager.ApplyControlsToProfile(playerID, profileIndex);
            GameManager.singleton.profilesManager.SaveProfiles();
            GameManager.singleton.profilesManager.RestoreDefaultControls(playerID);
            GameManager.singleton.profilesManager.ApplyProfileToPlayer(playerID,
                GameManager.singleton.localPlayerManager.GetPlayer(playerID).profile);
            GameManager.singleton.cMapper.onScreenClosed -= screenClosedEvent;
            Debug.Log($"Applied profile changes. Reapplied {GameManager.singleton.localPlayerManager.GetPlayer(playerID).profile} to player {playerID}.");
        }
    }
}