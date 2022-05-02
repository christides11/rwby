using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace rwby.ui.mainmenu
{
    public class OptionsProfilesMenu : MenuBase
    {
        [SerializeField] private Transform optionsTransform;
        [SerializeField] private GameObject optionPrefab;
        [SerializeField] private CanvasGroup canvasGroup;

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);

            foreach (Transform child in optionsTransform)
            {
                Destroy(child.gameObject);
            }
            FillProfiles();
        }

        private void FillProfiles()
        {
            var profiles = GameManager.singleton.profilesManager.Profiles;
            for(int i = 0; i < profiles.Count; i++)
            {
                int index = i;
                ProfileDefinition profile = profiles[i];
                GameObject profileButton = GameObject.Instantiate(optionPrefab, optionsTransform, false);
                profileButton.GetComponent<Selectable>().onSubmit.AddListener(() => { BUTTON_Profile(index); });
                profileButton.GetComponentInChildren<TextMeshProUGUI>().text = profile.profileName;
                profileButton.SetActive(true);
            }
            
            GameObject addProfileButton = GameObject.Instantiate(optionPrefab, optionsTransform, false);
            addProfileButton.GetComponent<Selectable>().onSubmit.AddListener(() => { BUTTON_AddProfile(); });
            addProfileButton.GetComponentInChildren<TextMeshProUGUI>().text = "+";
            addProfileButton.SetActive(true);
            
            GameObject backButton = GameObject.Instantiate(optionPrefab, optionsTransform, false);
            backButton.GetComponent<Selectable>().onSubmit.AddListener(() => { BUTTON_Back(); });
            backButton.GetComponentInChildren<TextMeshProUGUI>().text = "BACK";
            backButton.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }

        private UnityAction screenCloseAction;
        public void BUTTON_Profile(int index)
        {
            screenCloseAction = () => { AssignProfile(index); GameManager.singleton.cMapper.Close(false); };
            canvasGroup.interactable = false;
            GameManager.singleton.profilesManager.ApplyProfileToPlayer(0, index);
            GameManager.singleton.cMapper.onScreenClosed += screenCloseAction;
            GameManager.singleton.cMapper.Open();
        }

        private void AssignProfile(int profileIndex)
        {
            GameManager.singleton.cMapper.onScreenClosed -= screenCloseAction;
            GameManager.singleton.profilesManager.ApplyControlsToProfile(0, profileIndex);
            GameManager.singleton.profilesManager.SaveProfiles();
            canvasGroup.interactable = true;
        }

        public void BUTTON_AddProfile()
        {
            //GameManager.singleton.profilesManager.AddProfile("");
        }

        public void BUTTON_Back()
        {
            currentHandler.Back();
        }
    }
}