using System.Collections.Generic;
using rwby.ui.mainmenu;
using UnityEngine;

namespace rwby.ui
{
    public class CharacterSelectMenu : MenuBase
    {
        [System.Serializable]
        public class CSSConnection
        {
            public Selectable cssSelectable;
            [SerializeField] public ModGUIDContentReference characterContentReference 
                = new ModGUIDContentReference(new ContentGUID(8), 0, 0);
        }
        
        public List<CSSConnection> cssConnections = new List<CSSConnection>();
        public Selectable cssCustomButton;
        public Selectable endButton;

        public int charactersToSelect = 1;
        public List<ModGUIDContentReference> charactersSelected = new List<ModGUIDContentReference>();
        
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
        
        public void SelectCharacter(ModGUIDContentReference characterContentReference)
        {
            charactersSelected.Add(characterContentReference);
            if (charactersSelected.Count == charactersToSelect) EndSelection();
        }

        public void OpenCustomCharacterSelect()
        {
            
        }
    }
}