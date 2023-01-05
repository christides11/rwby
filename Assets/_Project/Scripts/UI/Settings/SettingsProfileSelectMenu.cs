using System;
using System.Collections;
using System.Collections.Generic;
using rwby.ui;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace rwby.ui
{
    public class SettingsProfileSelectMenu : MenuBase
    {
        private EventSystem eventSystem;
        private LocalPlayerManager localPlayerManager;
        
        [SerializeField] private SettingsProfilesMenu profilesMenu;
        
        public Transform profilesContentHolder;
        public ContentButtonBase profilePrefab;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            eventSystem = EventSystem.current;
            localPlayerManager = GameManager.singleton.localPlayerManager;
            gameObject.SetActive(true);
            
            RefreshProfilesList();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }

        private void Update()
        {
            if (UIHelpers.SelectDefaultSelectable(eventSystem, localPlayerManager.localPlayers[0]) 
                                             && profilesContentHolder.childCount > 0)
            {
                eventSystem.SetSelectedGameObject(profilesContentHolder.GetChild(0).gameObject);
            }
        }
        
        private void RefreshProfilesList()
        {
            foreach (Transform child in profilesContentHolder)
            {
                Destroy(child.gameObject);
            }

            var addP = GameObject.Instantiate(profilePrefab, profilesContentHolder, false);
            addP.label.text = "+ Create Profile";
            addP.onSubmit.AddListener(() => CreateProfile());

            for(int i = 0; i < GameManager.singleton.profilesManager.Profiles.Count; i++)
            {
                var profile = GameManager.singleton.profilesManager.Profiles[i];
                var profileIndex = i;
                var p = GameObject.Instantiate(profilePrefab, profilesContentHolder, false);
                p.label.text = profile.profileName;
                p.onSubmit.AddListener(() => OnProfileSelected(profileIndex));
            }
        }

        private void CreateProfile()
        {
            GameManager.singleton.profilesManager.AddProfile($"Profile {Random.Range(0, 999)}");
            RefreshProfilesList();
        }

        private void OnProfileSelected(int profileIndex)
        {
            int index = profileIndex;
            profilesMenu.currentSelectedProfile = profileIndex;
            profilesMenu.Forward((int)SettingsProfilesMenu.ProfilesSubMenuTypes.PROFILE_CUSTOMIZATION);
        }
    }
}