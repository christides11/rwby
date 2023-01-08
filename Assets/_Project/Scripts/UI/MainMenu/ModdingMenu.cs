using UnityEngine;
using ModIO;

namespace rwby.ui.mainmenu
{
    public class ModdingMenu : MenuBase
    {
        [SerializeField]private MainMenu mainMenu;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            ModIOBrowser.Browser.OpenBrowser(OnBrowserClosedAction);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }
        
        private void OnBrowserClosedAction()
        {
            mainMenu.Back();
        }

        private void GetMods()
        {
            var mods = ModIOUnity.GetSubscribedMods(out Result result);

            if (!result.Succeeded())
            {
                return;
            }

            foreach (var mod in mods)
            {
                if (mod.status == SubscribedModStatus.Installed)
                {
                    string dir = mod.directory;
                    Debug.Log($"{dir} : {mod.modProfile.name}");
                }
            }
        }
    }
}