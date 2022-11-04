using System.Collections;
using System.Collections.Generic;
using Rewired.Integration.UnityUI;
using TMPro;
using UnityEngine;

namespace rwby.ui
{
    public class PauseMainMenu : MenuBase
    {
        public PauseMenuInstance pauseMenuInstance;
        
        public Transform menuOptionParent;
        public ContentButtonBase buttonPrefab;

        public Dictionary<string, ContentButtonBase> optionsDictionary = new Dictionary<string, ContentButtonBase>();

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            AddOption("resume","RESUME", ResumeGame);
            AddOption("settings", "SETTINGS", OpenSettings);
            AddOption("exitLobby", "EXIT LOBBY", ExitLobby);
            AddOption("quitToDesktop", "QUIT TO DESKTOP", QuitToDesktop);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }

        public void AddOption(string id, string text, UnityEngine.Events.UnityAction callback, bool overrideOption = false)
        {
            if (optionsDictionary.ContainsKey(id))
            {
                if (!overrideOption) return;
                Destroy(optionsDictionary[id]);
                optionsDictionary.Remove(id);
            }
            var optionButton = GameObject.Instantiate(buttonPrefab, menuOptionParent, false);
            optionButton.label.text = text;
            optionButton.onSubmit.AddListener(callback);
            optionsDictionary.Add(id, optionButton);
        }
        
        private void ResumeGame()
        {
            pauseMenuInstance.pauseMenuHandler.CloseMenu(pauseMenuInstance.playerID, true);
        }
        
        private void OpenSettings()
        {
            pauseMenuInstance.Forward((int)PauseMenuInstance.PauseMenus.SETTINGS);
        }
        
        private void ExitLobby()
        {
            
        }
        
        private void QuitToDesktop()
        {
            Application.Quit();
        }
    }
}