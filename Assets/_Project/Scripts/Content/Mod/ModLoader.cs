using System.Collections.Generic;
using UnityEngine;
using UMod;
using System.IO;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Linq;
using UnityEngine.AddressableAssets.ResourceLocators;

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
        
        public delegate void ModActionDelegate(ModLoader modLoader, string modNamespace);
        public static event ModActionDelegate OnModLoaded;
        public static event ModActionDelegate OnModLoadUnsuccessful;

        public static string modsLoadedFileName = "loadedmods.json";
        public static readonly string bepinModsLoadedFileName = "enabled.txt";
        /// <summary>
        /// A list of all mods in the Mods folder.
        /// </summary>
        public List<ModInfo> modList = new List<ModInfo>();
        public Dictionary<string, ModInfo> modListByNamespace = new Dictionary<string, ModInfo>();
        /// <summary>
        /// A list of all currently enabled mods.
        /// </summary>
        public Dictionary<uint, LoadedModDefinition> loadedModsByID = new Dictionary<uint, LoadedModDefinition>();
        /// <summary>
        /// The path where mods are installed.
        /// </summary>
        public string modInstallPath = "";
        private ModDirectory modDirectory = null;

        public Dictionary<uint, string> modIDToNamespace = new Dictionary<uint, string>();
        public Dictionary<string, uint> modNamespaceToID = new Dictionary<string, uint>();

        public bool restartRequired = false;

        public async UniTask Initialize()
        {
            instance = this;

            // Initialize paths.
            modInstallPath = Path.Combine(Application.persistentDataPath, "Mods");
            
            Directory.CreateDirectory(modInstallPath);
            modDirectory = new ModDirectory(modInstallPath, true, false);
            Mod.DefaultDirectory = modDirectory;

            UpdateModList();
            await LoadLocalMod(modList[0]);
            await LoadModConfiguration();
            Debug.Log($"{loadedModsByID.Count} mods loaded");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F4))
            {
                foreach (var lm in loadedModsByID)
                {
                    if (lm.Value.definition == null) continue;
                    foreach (var cp in lm.Value.definition.ContentParsers)
                    {
                        foreach (var cpc in cp.Value.GUIDToInt)
                        {
                            Debug.Log(cpc.Key);
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                _ = LoadAllMods();
            }
        }

        public void SaveModConfiguration()
        {
            var loadedModListByNamespace = new HashSet<string>();
            var loadedModListFolderNames = new HashSet<string>();

            foreach(var l in loadedModsByID)
            {
                var modNamespace = modIDToNamespace[l.Key];
                loadedModListByNamespace.Add(modNamespace);

                var modPath = modListByNamespace[modNamespace].path;
                if (modPath != null)
                {
                    loadedModListFolderNames.Add(modPath.Segments.LastOrDefault());
                }
            }
            SaveLoadJsonService.Save<List<string>>(modsLoadedFileName, loadedModListByNamespace.ToList(), true);
            SaveLoadJsonService.Save(bepinModsLoadedFileName, string.Join('\n', loadedModListFolderNames));
        }

        private void UpdateModList()
        {
            modList.Clear();
            modListByNamespace.Clear();
            modIDToNamespace.Clear();
            modNamespaceToID.Clear();
            
            ModInfo modInfo = new ModInfo()
            {
                backingType = ModBackingType.Local,
                modNamespace = "christides11.rwby.core",
                modIdentifier = 1,
                modName = "core",
                path = null
            };
            modList.Add(modInfo);
            modListByNamespace.Add(modInfo.modNamespace, modInfo);
            modIDToNamespace.Add(modInfo.modIdentifier, modInfo.modNamespace);
            modNamespaceToID.Add(modInfo.modNamespace, modInfo.modIdentifier);
            
            FindUModMods();
            FindOtherMods();
        }

        private void FindUModMods()
        {
            // Create a list of the mods we have in the mod directory.
            if (modDirectory.HasMods)
            {
                foreach (string modName in modDirectory.GetModNames())
                {
                    string infoFilePath = Path.Combine(Path.GetDirectoryName(modDirectory.GetModPath(modName).AbsolutePath)!, "info.json");
                    if (File.Exists(infoFilePath) == false) continue;
                    if (!SaveLoadJsonService.TryLoad(infoFilePath, out AddressablesInfoFile aif)) continue;
                    
                    IModInfo modInfo = modDirectory.GetMod(modName);
                    ModInfo mi = new ModInfo
                    {
                        backingType = ModBackingType.UMod,
                        commandLine = false,
                        path = modDirectory.GetModPath(modName),
                        modName = modInfo.NameInfo.ModName,
                        modNamespace = $"{aif.modIdentifier}",
                        modIdentifier = aif.modID,
                        disableRequiresRestart = aif.disableRequiresRestart,
                        enableRequiresRestart = aif.enableRequiresRestart
                    };
                    modList.Add(mi);
                    modListByNamespace.Add(mi.modNamespace, mi);
                    modIDToNamespace.Add(aif.modID, aif.modIdentifier);
                    modNamespaceToID.Add(aif.modIdentifier, aif.modID);
                }
            }

            // Add mods from the command line.
            if (Mod.CommandLine.HasMods)
            {
                foreach (Uri modPath in Mod.CommandLine.AllMods)
                {
                    string infoFilePath = Path.Combine(Path.GetDirectoryName(modPath.AbsolutePath)!, "info.json");
                    if (File.Exists(infoFilePath) == false) continue;
                    if (!SaveLoadJsonService.TryLoad(infoFilePath, out AddressablesInfoFile aif)) continue;
                    IModInfo modInfo = ModDirectory.GetMod(new FileInfo(modPath.LocalPath));
                    ModInfo mi = new ModInfo
                    {
                        backingType = ModBackingType.UMod,
                        commandLine = true,
                        path = new Uri(modPath.AbsolutePath),
                        modName = modInfo.NameInfo.ModName,
                        modNamespace = $"{aif.modIdentifier}",
                        modIdentifier = aif.modID,
                        disableRequiresRestart = aif.disableRequiresRestart,
                        enableRequiresRestart = aif.enableRequiresRestart
                    };
                    modList.Add(mi);
                    modListByNamespace.Add(mi.modNamespace, mi);
                    modIDToNamespace.Add(aif.modID, aif.modIdentifier);
                    modNamespaceToID.Add(aif.modIdentifier, aif.modID);
                }
            }
        }

        private void FindOtherMods()
        {
            string[] foldersInDirectory = Directory.GetDirectories(modInstallPath);
            foreach (string folderPath in foldersInDirectory)
            {
                string infoFilePath = Path.Combine(folderPath, "info.json");
                if (File.Exists(infoFilePath) == false) continue;
                if (!SaveLoadJsonService.TryLoad(infoFilePath, out AddressablesInfoFile aif)) continue;
                if (aif.backingType == ModBackingType.UMod) continue;
                
                ModInfo mi = new ModInfo
                {
                    backingType = aif.backingType,
                    commandLine = false,
                    path = new Uri(folderPath),
                    modName = $"{aif.modName}",
                    modNamespace = $"{aif.modIdentifier}",
                    modIdentifier = aif.modID,
                    disableRequiresRestart = aif.disableRequiresRestart,
                    enableRequiresRestart = aif.enableRequiresRestart
                };
                modList.Add(mi);
                modListByNamespace.Add(mi.modNamespace, mi);
                modIDToNamespace.Add(aif.modID, aif.modIdentifier);
                modNamespaceToID.Add(aif.modIdentifier, aif.modID);
            }
        }

        /// <summary>
        /// Check to see if any mods have loaded that we didn't catch.
        /// These will usually be dependencies since these get loaded automatically.
        /// </summary>
        private async UniTask CheckForLoadedUModDependencies()
        {
            //TODO
            /*
            foreach (ModInfo mi in modList)
            {
                if (mi.backingType != ModBackingType.UMod) continue;

                if (ModHost.IsModInUse(mi.path) && !loadedMods.ContainsKey(new ModIdentifierTuple(mi.)))
                {
                    await LoadMod(mi);
                }
            }*/
        }

        public async UniTask LoadModConfiguration()
        {
            if(!SaveLoadJsonService.TryLoad(modsLoadedFileName, out List<string> savedLoadedMods)) savedLoadedMods = new List<string>();

            List<string> failedToLoadMods = await LoadMods(savedLoadedMods);
            foreach (string um in failedToLoadMods)
            {
                savedLoadedMods.Remove(um);
            }
        }

        #region Loading
        public async UniTask LoadAllMods()
        {
            foreach (ModInfo mi in modList)
            {
                await LoadMod(mi);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifiers"></param>
        /// <returns>Returns a list of mods that failed to load.</returns>
        public async UniTask<List<string>> LoadMods(List<string> identifiers)
        {
            List<string> notLoaded = new List<string>();
            for (int i = 0; i < identifiers.Count; i++)
            {
                if (!(await LoadMod(identifiers[i])))
                {
                    notLoaded.Add(identifiers[i]);
                }
            }
            return notLoaded;
        }

        public async UniTask<bool> LoadMod(string modNamespace)
        {
            if (modList.Exists(x => x.modNamespace == modNamespace))
            {
                var r = await LoadMod(modList.Find(x => x.modNamespace == modNamespace));
                OnModLoaded?.Invoke(this, modNamespace);
                return r;
            }
            OnModLoadUnsuccessful?.Invoke(this, modNamespace);
            return false;
        }

        private async UniTask<bool> LoadMod(ModInfo modInfo)
        {
            switch (modInfo.backingType)
            {
                case ModBackingType.Addressables:
                    return await LoadAddressablesMod(modInfo);
                case ModBackingType.UMod:
                    return await LoadUModMod(modInfo);
                case ModBackingType.BepInEx:
                    return LoadBepInExMod(modInfo);
                case ModBackingType.Local:
                    return true;
            }

            return false;
        }

        private bool LoadBepInExMod(ModInfo modInfo)
        {
            if (loadedModsByID.ContainsKey(modInfo.modIdentifier)) return true;
            loadedModsByID.Add(modInfo.modIdentifier, new LoadedBepInExModDefinition()
            {
                definition = null
            });
            return true;
        }

        private async UniTask LoadLocalMod(ModInfo modInfo)
        {
            var handle = Addressables.LoadAssetAsync<AddressablesModDefinition>("moddefinition");
            await handle;
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed) return;

            LoadedLocalAddressablesModDefinition loadedModDefinition = new LoadedLocalAddressablesModDefinition
            {
                definition = handle.Result,
                handle = handle
            };
            loadedModsByID.Add(modInfo.modIdentifier, loadedModDefinition);
        }

        private async UniTask<bool> LoadUModMod(ModInfo modInfo)
        {
            ModHost mod = null;
            try
            {
                mod = Mod.Load(modInfo.path);
                if (mod.IsModLoaded == false) throw new Exception($"UMod mod failed to load: {mod.LoadResult.Error}");

                ModAsyncOperation mao = mod.Assets.LoadAsync("ModDefinition");
                await mao;
                if (mao.IsSuccessful == false) return false;
                
                LoadedUModModDefinition loadedModDefinition = new LoadedUModModDefinition()
                {
                    definition = mao.Result as IModDefinition,
                    host = mod
                };
                loadedModsByID.Add(loadedModDefinition.definition.ModID, loadedModDefinition);
                await CheckForLoadedUModDependencies();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed loading UMod mod {mod} {modInfo.modNamespace} {modInfo.path}: {e.Message}");
                if (mod.IsModLoaded)
                {
                    mod.UnloadMod();
                }
                await CheckForLoadedUModDependencies();
                return false;
            }
        }

        private async UniTask<bool> LoadAddressablesMod(ModInfo modInfo)
        {
            try
            {
                var handle = Addressables.LoadContentCatalogAsync(Path.Combine(modInfo.path.AbsolutePath, "catalog.json"), false);
                ResourceLocationMap loadResult = await handle as ResourceLocationMap;
                IModDefinition imd = null;
                foreach (var key in loadResult.Keys)
                {
                    if (typeof(IModDefinition).IsAssignableFrom(loadResult.Locations[key][0].ResourceType))
                    {
                        imd = await Addressables.LoadAssetAsync<IModDefinition>(loadResult.Locations[key][0]);
                        Debug.Log($"Test: {imd.ModID}");
                        break;
                    }
                }

                LoadedAddressablesModDefinition loadedModDefinition = new LoadedAddressablesModDefinition
                {
                    definition = imd,
                    resourceLocatorHandle = handle,
                    resourceLocationMap = loadResult
                };
                loadedModsByID.Add(loadedModDefinition.definition.ModID, loadedModDefinition);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed loading Addressables mod {modInfo.modNamespace}: {e.Message}");
                return false;
            }
        }
        #endregion

        #region Unloading

        public void UnloadMod(string modNamespace)
        {
            if (!modNamespaceToID.ContainsKey(modNamespace)) return;
            UnloadMod(modNamespaceToID[modNamespace]);
        }

        public void UnloadMod(uint modID)
        {
            if (!loadedModsByID.ContainsKey(modID)) return;
            loadedModsByID[modID].Unload();
            loadedModsByID.Remove(modID);
        }
        #endregion

        public bool TryGetLoadedMod(string modID, out LoadedModDefinition loadedMod)
        {
            loadedMod = null;
            if (!modNamespaceToID.ContainsKey(modID)) return false;
            if (!TryGetLoadedMod(modNamespaceToID[modID], out loadedMod)) return false;
            return true;
        }
        
        public bool TryGetLoadedMod(uint modID, out LoadedModDefinition loadedMod)
        {
            if (!loadedModsByID.TryGetValue(modID, out loadedMod)) return false;
            return true;
        }

        public bool IsLoaded(uint modID)
        {
            return loadedModsByID.ContainsKey(modID);
        }

        public ModInfo GetModInfo(uint modID)
        {
            if (!modIDToNamespace.ContainsKey(modID)) return null;
            return GetModInfo(modIDToNamespace[modID]);
        }
        
        public ModInfo GetModInfo(string modNamespace)
        {
            if (!modListByNamespace.ContainsKey(modNamespace)) return null;
            return modListByNamespace[modNamespace];
        }
    }
}