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

        public static string modsLoadedFileName = "loadedmods.json";
        public static readonly string bepinModsLoadedFileName = "enabled.txt";
        /// <summary>
        /// A list of all mods in the Mods folder.
        /// </summary>
        public List<ModInfo> modList = new List<ModInfo>();
        /// <summary>
        /// A list of all currently enabled mods.
        /// </summary>
        public Dictionary<ContentGUID, LoadedModDefinition> loadedModsByGUID = new Dictionary<ContentGUID, LoadedModDefinition>();
        public Dictionary<string, ModInfo> loadedModsByIdentifier = new Dictionary<string, ModInfo>();
        /// <summary>
        /// The path where mods are installed.
        /// </summary>
        public string modInstallPath = "";
        private ModDirectory modDirectory = null;

        public async UniTask Initialize()
        {
            instance = this;

            modList.Add(new ModInfo()
            {
                backingType = ModBackingType.Local,
                fileName = "",
                identifier = "christides11.rwby.core",
                modName = "core",
                path = new Uri("/")
            });
            
            // Initialize paths.
            modInstallPath = Path.Combine(Application.persistentDataPath, "Mods");
            
            Directory.CreateDirectory(modInstallPath);
            modDirectory = new ModDirectory(modInstallPath, true, false);
            Mod.DefaultDirectory = modDirectory;

            UpdateModList();
            await LoadLocalMod();
            await LoadModConfiguration();
            Debug.Log($"{loadedModsByIdentifier.Count} mods loaded");
        }

        private void UpdateModList()
        {
            modList.Clear();
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
                    string infoFilePath = Path.Combine(Path.GetDirectoryName(modDirectory.GetModPath(modName).AbsolutePath), "info.json");
                    if (File.Exists(infoFilePath) == false) continue;
                    if (!SaveLoadJsonService.TryLoad(infoFilePath, out AddressablesInfoFile aif)) continue;
                    
                    IModInfo modInfo = modDirectory.GetMod(modName);
                    ModInfo mi = new ModInfo
                    {
                        backingType = ModBackingType.UMod,
                        commandLine = false,
                        path = new Uri(Path.GetDirectoryName(modDirectory.GetModPath(modName).AbsolutePath)),
                        fileName = modName,
                        modName = modInfo.NameInfo.ModName,
                        identifier = $"{aif.modIdentifier}",
                        disableRequiresRestart = aif.disableRequiresRestart,
                        enableRequiresRestart = aif.enableRequiresRestart
                    };
                    modList.Add(mi);
                }
            }

            // Add mods from the command line.
            if (Mod.CommandLine.HasMods)
            {
                foreach (Uri modPath in Mod.CommandLine.AllMods)
                {
                    string infoFilePath = Path.Combine(Path.GetDirectoryName(modPath.AbsolutePath), "info.json");
                    if (File.Exists(infoFilePath) == false) continue;
                    if (!SaveLoadJsonService.TryLoad(infoFilePath, out AddressablesInfoFile aif)) continue;
                    IModInfo modInfo = ModDirectory.GetMod(new FileInfo(modPath.LocalPath));
                    ModInfo mi = new ModInfo
                    {
                        backingType = ModBackingType.UMod,
                        commandLine = true,
                        path = new Uri(Path.GetDirectoryName(modPath.AbsolutePath)),
                        fileName = System.IO.Path.GetFileName(modPath.LocalPath),
                        modName = modInfo.NameInfo.ModName,
                        identifier = $"{aif.modIdentifier}",
                        disableRequiresRestart = aif.disableRequiresRestart,
                        enableRequiresRestart = aif.enableRequiresRestart
                    };
                    modList.Add(mi);
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
                    fileName = "info.json",
                    modName = $"{aif.modName}",
                    identifier = $"{aif.modIdentifier}",
                    disableRequiresRestart = aif.disableRequiresRestart,
                    enableRequiresRestart = aif.enableRequiresRestart
                };
                modList.Add(mi);
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

        public void SaveModConfiguration()
        {
            SaveLoadJsonService.Save(modsLoadedFileName, JsonUtility.ToJson(loadedModsByIdentifier.Keys.ToList()));
            //SaveLoadJsonService.Save(bepinModsLoadedFileName, string.Join('\n', loadedBepInExMods));
        }

        public async UniTask LoadModConfiguration()
        {
            if(!SaveLoadJsonService.TryLoad(modsLoadedFileName, out List<string> savedLoadedMods)) savedLoadedMods = new List<string>();

            List<string> failedToLoadMods = await LoadMods(savedLoadedMods);
            foreach (string um in failedToLoadMods)
            {
                savedLoadedMods.Remove(um);
            }

            /*
            if (!SaveLoadJsonService.TryLoadFile(bepinModsLoadedFileName, out string bepinModsString)) bepinModsString = "";
            var strSeparated = bepinModsString.Split('\n');

            loadedBepInExMods = new HashSet<string>(strSeparated);*/
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

        public async UniTask<bool> LoadMod(string identifier)
        {
            if (modList.Exists(x => x.identifier == identifier))
            {
                return await LoadMod(modList.Find(x => x.identifier == identifier));
            }
            return false;
        }

        public async UniTask<bool> LoadMod(ModInfo modInfo)
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
                    return false;
            }

            return false;
        }

        private bool LoadBepInExMod(ModInfo modInfo)
        {
            loadedModsByIdentifier.Add(modInfo.identifier, modInfo);
            //loadedBepInExMods.Add( modInfo.path.Segments[^1] );
            return true;
        }

        private async UniTask LoadLocalMod()
        {
            var handle = Addressables.LoadAssetAsync<AddressablesModDefinition>("moddefinition");
            await handle;
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed) return;

            LoadedLocalAddressablesModDefinition loadedModDefinition = new LoadedLocalAddressablesModDefinition
            {
                definition = handle.Result,
                handle = handle
            };
            loadedModsByIdentifier.Add(modList[0].identifier, modList[0]);
            loadedModsByGUID.Add(handle.Result.ModGUID, loadedModDefinition);
        }

        private async UniTask<bool> LoadUModMod(ModInfo modInfo)
        {
            ModHost mod = Mod.Load(modInfo.path);
            try
            {
                if (mod.IsModLoaded == false) throw new Exception($"UMod mod failed to load: {mod.LoadResult.Error}");

                ModAsyncOperation mao = mod.Assets.LoadAsync("ModDefinition");
                await mao;
                if (mao.IsSuccessful == false) return false;
                
                LoadedUModModDefinition loadedModDefinition = new LoadedUModModDefinition()
                {
                    definition = mao.Result as IModDefinition,
                    host = mod
                };
                loadedModsByIdentifier.Add(modInfo.identifier, modInfo);
                loadedModsByGUID.Add(loadedModDefinition.definition.ModGUID, loadedModDefinition);
                await CheckForLoadedUModDependencies();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed loading UMod mod {modInfo.identifier}: {e.Message}");
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
                        Debug.Log($"Test: {imd.ModGUID}");
                        break;
                    }
                }

                LoadedAddressablesModDefinition loadedModDefinition = new LoadedAddressablesModDefinition
                {
                    definition = imd,
                    resourceLocatorHandle = handle,
                    resourceLocationMap = loadResult
                };
                loadedModsByIdentifier.Add(modInfo.identifier, modInfo);
                loadedModsByGUID.Add(loadedModDefinition.definition.ModGUID, loadedModDefinition);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed loading Addressables mod {modInfo.identifier}: {e.Message}");
                return false;
            }
        }
        #endregion

        #region Unloading

        public void UnloadMod(string identifier)
        {
            if (!loadedModsByIdentifier.ContainsKey(identifier)) return;
        }
        /*public void UnloadMod(ContentGUID modGUID)
        {
            if (loadedModsByGUID.ContainsKey(modGUID)) return;

            // Unload mod
            loadedModsByGUID[modGUID].Unload();
            loadedModsByGUID.Remove(modGUID);
        }

        public virtual void UnloadAllMods()
        {
            foreach (var k in loadedModsByGUID.Keys)
            {
                loadedModsByGUID[k].Unload();
            }
            loadedModsByGUID.Clear();
        }*/
        #endregion

        public bool TryGetLoadedMod(ContentGUID modGUID, out LoadedModDefinition loadedMod)
        {
            if (!loadedModsByGUID.TryGetValue(modGUID, out loadedMod)) return false;
            return true;
        }

        public bool IsLoaded(ContentGUID modGUID)
        {
            return loadedModsByGUID.ContainsKey(modGUID);
        }

        public IModInfo GetModInfo(ModInfo modInfo)
        {
            return ModDirectory.GetMod(new FileInfo(modInfo.path.LocalPath));
        }
    }
}