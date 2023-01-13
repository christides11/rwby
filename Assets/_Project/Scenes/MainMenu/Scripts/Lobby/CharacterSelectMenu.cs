using System.Collections.Generic;
using rwby.ui.mainmenu;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby.ui
{
    public class CharacterSelectMenu : MenuBase
    {
        [System.Serializable]
        public class CSSConnection
        {
            public Selectable cssSelectable;

            [SerializeField] public ModContentStringReference characterContentReference
                = new ModContentStringReference();
        }
        
        public GameObject defaultSelectedUIItem;
        
        public List<CSSConnection> cssConnections = new List<CSSConnection>();
        public Selectable cssCustomButton;
        public Selectable endButton;

        public int charactersToSelect = 1;
        public List<ModIDContentReference> charactersSelected = new List<ModIDContentReference>();
        
        public delegate void EmptyDelegate();
        public event EmptyDelegate OnCharactersSelected;
        
        public void Awake()
        {
            for (int i = 0; i < cssConnections.Count; i++)
            {
                int temp = i;
                cssConnections[i].cssSelectable.onSubmit.AddListener(() => { SelectCharacter(cssConnections[temp].characterContentReference); });
            }
            cssCustomButton.onSubmit.AddListener(OpenCustomCharacterSelect);
            endButton.onSubmit.AddListener(EndSelection);
        }

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            charactersSelected.Clear();
            gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(defaultSelectedUIItem);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }

        public void Refresh()
        {
            
        }

        public void EndSelection()
        {
            OnCharactersSelected?.Invoke();
            currentHandler.Back();
        }
        
        public void SelectCharacter(ModContentStringReference characterContentReference)
        {
            charactersSelected.Add(ContentManager.singleton.ConvertStringToGUIDReference(characterContentReference));
            if (charactersSelected.Count == charactersToSelect) EndSelection();
        }

        public void OpenCustomCharacterSelect()
        {
            
        }
    }
}