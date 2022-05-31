using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace rwby
{
    public class PauseMenu : MonoBehaviour
    {
        public delegate void EmptyAction();
        public static event EmptyAction onOpened;

        public static PauseMenu singleton;

        public bool paused = false;

        public GameObject canvas;
        public Transform menuOptionParent;
        public GameObject menuOptionItem;

        public IClosableMenu currentSubmenu;

        private void Awake()
        {
            singleton = this;
        }

        public void Open()
        {
            paused = true;
            currentSubmenu = null;

            foreach(Transform child in menuOptionParent.transform)
            {
                Destroy(child.gameObject);
            }

            AddOption("Resume", (a) => { Close(); });
            onOpened?.Invoke();
            AddOption("Exit Session", (a) => { });

            canvas.SetActive(true);
        }

        public void Close()
        {
            if(currentSubmenu != null)
            {
                bool result = currentSubmenu.TryClose();
                if (result == true)
                {
                    currentSubmenu = null;
                }
                //PlayerPointerHandler.singleton.ShowMice();
                return;
            }

            canvas.SetActive(false);
            paused = false;

            //PlayerPointerHandler.singleton.HideMice();
        }

        public void AddOption(string text, UnityEngine.Events.UnityAction<Rewired.Integration.UnityUI.PlayerPointerEventData> callback)
        {
            GameObject optionButton = GameObject.Instantiate(menuOptionItem, menuOptionParent, false);
            optionButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
            optionButton.GetComponent<PlayerPointerEventTrigger>().OnPointerClickEvent.AddListener(callback);
        }
    }
}