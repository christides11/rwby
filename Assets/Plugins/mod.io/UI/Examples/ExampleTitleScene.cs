﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModIOBrowser;
using ModIOBrowser.Implementation;
using TMPro;
using ModIO.Implementation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ModIO.Utility;

namespace Plugins.mod.io.UI.Examples
{

    public class ExampleTitleScene : MonoBehaviour
    {
        [SerializeField] Selectable DefaultSelection;
        [SerializeField] private ExampleSettingsPanel exampleSettingsPanel;
        public string verticalControllerInput = "Vertical";
        public List<string> mouseInput = new List<string>();
        public MultiTargetDropdown languageSelectionDropdown;

    void Start()
    {
        OpenTitle();

            languageSelectionDropdown.gameObject.SetActive(false);
            StartCoroutine(SetupTranslationDropDown());
        }

        IEnumerator SetupTranslationDropDown()
        {
            //Wait until Translation manager is all set up
            yield return new WaitForEndOfFrame();

            languageSelectionDropdown.gameObject.SetActive(true);
            languageSelectionDropdown.ClearOptions();

            languageSelectionDropdown.AddOptions(Enum.GetNames(typeof(TranslatedLanguages))
                .Select(x => new TMP_Dropdown.OptionData(x.ToString()))
                .ToList());

            languageSelectionDropdown.value = (int)TranslationManager.Instance.SelectedLanguage;
        }

        public void OnTranslationDropdownChange()
        {
            TranslationManager.Instance.ChangeLanguage((TranslatedLanguages)languageSelectionDropdown.value);
        }

        public void OpenMods()
        {
            // Assign the 'GoBackToTitleScene' method as the onClose method so we can maintain a focused
            // selectable highlight if we're on controller
            Browser.Instance.gameObject.SetActive(true);
            Browser.OpenBrowser(OpenTitle);
            gameObject.transform.parent.gameObject.SetActive(false);
        }

        public void OpenSettings()
        {
            exampleSettingsPanel.ActivatePanel(true);
        }

    public void OpenTitle()
    {
        //Browser.Instance.gameObject needs to stay on so that translations, glyphsettings etc
        //can initialize
        gameObject.transform.parent.gameObject.SetActive(true);
        DefaultSelection.Select();
    }

        public void Quit()
        {
            Application.Quit();
        }

        public void DeselectOtherTitles()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void Update()
        {
            if(Input.GetAxis(verticalControllerInput) != 0f)
            {
                //Hide mouse
                Cursor.lockState = CursorLockMode.Locked;

                if(EventSystem.current.currentSelectedGameObject == null)
                {
                    DefaultSelection.Select();
                }
            }
            else if(mouseInput.Any(x => Input.GetAxis(x) != 0))
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
