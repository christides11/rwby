using System.Collections;
using System.Collections.Generic;
using rwby.ui.mainmenu;
using UnityEngine;

namespace rwby.ui
{
    public class SettingsProfilesMenu : MenuBase
    {
        public GameObject profilesListMenu;
        public GameObject rebindMenu;

        public Transform profilesContentHolder;
        public ContentButtonBase profilePrefab;
        
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

            foreach (var profile in GameManager.singleton.profilesManager.Profiles)
            {
                var cachedProfile = profile;
                var p = GameObject.Instantiate(profilePrefab, profilesContentHolder, false);
                p.label.text = profile.profileName;
                p.onSubmit.AddListener(() => OnProfileSelected(cachedProfile));
            }
        }

        private void OnProfileSelected(ProfileDefinition profile)
        {
            Debug.Log(profile.profileName);
        }
    }
}