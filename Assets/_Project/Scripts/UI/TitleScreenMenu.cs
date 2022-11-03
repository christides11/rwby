using Rewired;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby.ui.mainmenu
{
    public class TitleScreenMenu : MainMenuMenu
    {
        [FormerlySerializedAs("modeSelectMenu")] public MainMenu mainMenu;

        [ActionIdProperty(typeof(Action))]
        public int[] validActions;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < validActions.Length; i++)
            {
                if (ReInput.players.SystemPlayer.GetButton(validActions[i]))
                {
                    NextMenu();
                    return;
                }
            }

            if (Input.anyKey)
            {
                NextMenu();
                return;
            }
        }

        private void NextMenu()
        {
            currentHandler.Forward((int)MainMenuType.MAIN_MENU);
            gameObject.SetActive(false);
        }
    }
}