using System.IO;
using ModIO;
using ModIOBrowser;
using UnityEngine;

namespace rwby.ui.mainmenu
{
    public class ModdingModIOMenu : MenuBase
    {
        [SerializeField] private Browser modioBrowser;
        [SerializeField] private ModdingMenu moddingMenu;
        
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
            GetMods();
            moddingMenu.CloseModBrowser();
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
                    Copy(dir, 
                        Path.Combine(
                            ModLoader.instance.modInstallPath, 
                            "modio_" + Path.GetFileName(Path.GetDirectoryName(dir+"/a.txt")
                            )));
                }
            }
        }
        
        private void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);
            CopyAll(diSource, diTarget);
        }

        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
