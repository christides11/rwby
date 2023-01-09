using TMPro;
using UnityEngine;

namespace rwby.ui.mainmenu
{
    public class ModdingMenu : MenuBase
    {
        [SerializeField]private MainMenu mainMenu;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Menus")]
        [SerializeField] private ModdingModIOMenu modIOMenu;

        [Header("Configure Options")] 
        [SerializeField] private TMP_InputField searchInputField;
        [SerializeField] private Transform modListContentHolder;
        [SerializeField] private GameObject modListContentPrefab;
        [SerializeField] private Color modListContentOnColor;
        [SerializeField] private Color modListContentOffColor;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            canvasGroup.interactable = true;
            searchInputField.onEndEdit.RemoveAllListeners();
            searchInputField.onEndEdit.AddListener(WhenSearchInputChanged);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }
        
        public void BUTTON_ModBrowser()
        {
            canvasGroup.interactable = false;
            modIOMenu.Open(MenuDirection.FORWARDS, null);
        }

        public void CloseModBrowser()
        {
            modIOMenu.TryClose(MenuDirection.BACKWARDS);
            canvasGroup.interactable = true;
        }
        
        private void WhenSearchInputChanged(string arg0)
        {
            
        }
    }
}