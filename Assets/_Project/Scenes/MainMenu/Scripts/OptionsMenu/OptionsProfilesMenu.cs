using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace rwby.ui.mainmenu
{
    public class OptionsProfilesMenu : MenuBase
    {
        [SerializeField] private Transform optionsTransform;
        [SerializeField] private GameObject optionPrefab;

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
            GameObject defaultProfile = GameObject.Instantiate(optionPrefab, optionsTransform, false);
            defaultProfile.GetComponentInChildren<TextMeshProUGUI>().text = "DEFAULT";
            defaultProfile.SetActive(true);
            
            
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

        public void BUTTON_Profile(int index)
        {
            
        }

        public void BUTTON_Back()
        {
            currentHandler.Back();
        }
    }
}