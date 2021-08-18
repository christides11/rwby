using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMod;
using System.IO;
using System;
//using Newtonsoft.Json;
using System.Security.AccessControl;

namespace rwby
{
    /// <summary>
    /// Handles loading and unloading mods, along with keeping track of what is
    /// currently installed.
    /// </summary>
    [System.Serializable]
    public class ModLoader : MonoBehaviour
    {
        public static ModLoader instance;

        public static string modsLoadedFileName = "loadedmods.json";
        /// <summary>
        /// A list of all mods in the Mods folder.
        /// </summary>
        public List<ModInfo> modList = new List<ModInfo>();
        /// <summary>
        /// A list of all currently enabled mods.
        /// </summary>
        public Dictionary<string, LoadedModDefinition> loadedMods = new Dictionary<string, LoadedModDefinition>();
        /// <summary>
        /// The path where mods are installed.
        /// </summary>
        private string modInstallPath = "";
        private ModDirectory modDirectory = null;

        protected bool inited;

        public virtual void Initialize()
        {
            instance = this;
            modInstallPath = Path.Combine(Application.persistentDataPath, "Mods");
            Directory.CreateDirectory(modInstallPath);
            modDirectory = new ModDirectory(modInstallPath, true, false);
            Mod.DefaultDirectory = modDirectory;
            inited = true;
            //ConsoleWindow.current.WriteLine($"ModLoader initialized. Path: {modInstallPath}");
            Debug.Log($"ModLoader initialized. Path: {modInstallPath}");
            UpdateModList();
            List<string> loadedMods = SaveLoadJsonService.Load<List<string>>("");
            if (loadedMods == null)
            {
                loadedMods = new List<string>();
            }
            List<string> unloadedMods = LoadMods(loadedMods);
            foreach (string um in unloadedMods)
            {
                loadedMods.Remove(um);
            }
            SaveLoadJsonService.Save(modsLoadedFileName, JsonUtility.ToJson(loadedMods));
            LoadAllMods();
        }

        public virtual void UpdateModList()
        {
            if (!inited)
            {
                //ConsoleWindow.current.WriteLine("ModLoader was not initialized, can't update mod list.");
                return;
            }

            modList.Clear();

            // Create a list of the mods we have in the mod directory.
            if (modDirectory.HasMods)
            {
                foreach (string modName in modDirectory.GetModNames())
                {
                    ModInfo mi = new ModInfo();
                    mi.path = modDirectory.GetModPath(modName);
                    mi.fileName = modName;
                    IModInfo modInfo = modDirectory.GetMod(mi.fileName);
                    mi.modName = modInfo.NameInfo.ModName;
                    mi.identifier = $"{modInfo.ModAuthor.ToLower()}.{modInfo.NameInfo.ModName.ToLower()}";
                    modList.Add(mi);
                }
            }

            // Add mods from the command line.
            if (Mod.CommandLine.HasMods)
            {
                foreach (Uri modPath in Mod.CommandLine.AllMods)
                {
                    ModInfo mi = new ModInfo();
                    mi.commandLine = true;
                    mi.path = modPath;
                    mi.fileName = System.IO.Path.GetFileName(modPath.LocalPath);
                    IModInfo modInfo = ModDirectory.GetMod(new FileInfo(mi.path.LocalPath));
                    mi.modName = modInfo.NameInfo.ModName;
                    mi.identifier = $"{modInfo.ModAuthor.ToLower()}.{modInfo.NameInfo.ModName.ToLower()}";
                    modList.Add(mi);
                }
            }
        }

        /// <summary>
        /// Check to see if any mods have loaded that we didn't catch.
        /// These will usually be dependencies since these get loaded automatically.
        /// </summary>
        protected virtual void CheckLoadedModList()
        {
            foreach (ModInfo mi in modList)
            {
                if (ModHost.IsModInUse(mi.path) && !loadedMods.ContainsKey(mi.identifier))
                {
                    //ConsoleWindow.current.WriteLine($"Found stray mod {mi.identifier}.");
                    LoadMod(mi);
                }
            }
        }

        #region Loading
        public virtual void LoadAllMods()
        {
            int startValue = loadedMods.Count;
            foreach (ModInfo mi in modList)
            {
                LoadMod(mi);
            }
            Debug.Log($"Loaded {loadedMods.Count-startValue} mods.");
        }

        public virtual bool LoadMod(string identifier)
        {
            if (modList.Exists(x => x.identifier == identifier))
            {
                return LoadMod(modList.Find(x => x.identifier == identifier));
            }
            return false;
        }

        public virtual List<string> LoadMods(List<string> identifiers)
        {
            List<string> notLoaded = new List<string>();
            for (int i = 0; i < identifiers.Count; i++)
            {
                if (!LoadMod(identifiers[i]))
                {
                    notLoaded.Add(identifiers[i]);
                }
            }
            return notLoaded;
        }

        public virtual bool LoadMod(ModInfo modInfo)
        {
            if (loadedMods.ContainsKey(modInfo.identifier))
            {
                //ConsoleWindow.current.WriteLine($"Mod {modInfo.identifier} is already loaded.");
                return false;
            }

            ModHost mod = Mod.Load(modInfo.path);

            try
            {
                if (mod.IsModLoaded)
                {
                    if (mod.Assets.Exists("ModDefinition"))
                    {
                        IModDefinition modDefinition = mod.Assets.Load("ModDefinition") as IModDefinition;
                        loadedMods.Add(modInfo.identifier, new LoadedModDefinition(mod, modDefinition));
                        //ConsoleWindow.current.WriteLine($"Loaded mod {modInfo.identifier}.");
                        CheckLoadedModList();
                        return true;
                    }
                    throw new Exception($"No ModDefinition found.");
                }
                throw new Exception($"{mod.LoadResult.Error}");
            }
            catch (Exception e)
            {
                //ConsoleWindow.current.WriteLine($"Failed loading mod {modInfo.identifier}: {e.Message}");
                if (mod.IsModLoaded)
                {
                    mod.UnloadMod();
                }
                CheckLoadedModList();
                return false;
            }
        }
        #endregion

        #region Unloading
        public virtual bool UnloadMod(string modIdentifier)
        {
            if (loadedMods.ContainsKey(modIdentifier))
            {
                // Unload mod
                (loadedMods[modIdentifier].host as ModHost).UnloadMod();
                loadedMods.Remove(modIdentifier);
                return true;
            }
            return false;
        }

        public virtual void UnloadAllMods()
        {
            foreach (string k in loadedMods.Keys)
            {
                // Ignore addressable mods.
                if(loadedMods[k].host == null)
                {
                    continue;
                }
                (loadedMods[k].host as ModHost).UnloadMod();
            }
            loadedMods.Clear();
        }
        #endregion

        public bool IsLoaded(string modIdentifier)
        {
            return loadedMods.ContainsKey(modIdentifier);
        }

        public IModInfo GetModInfo(ModInfo modInfo)
        {
            return ModDirectory.GetMod(new FileInfo(modInfo.path.LocalPath));
        }
    }
}