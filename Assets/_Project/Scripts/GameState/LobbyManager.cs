using Cysharp.Threading.Tasks;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace rwby
{
    public class LobbyManager : NetworkBehaviour
    {
        public delegate void EmptyAction();
        public static event EmptyAction OnLobbyManagerSpawned;
        public static event EmptyAction OnLobbyPlayersUpdated;
        public static event EmptyAction OnLobbySettingsChanged;
        public static event EmptyAction OnCurrentGamemodeChanged;
        public static event EmptyAction OnGamemodeSettingsChanged;

        public static LobbyManager singleton;
        public LobbySettingsManager settings;
        
        [Networked(OnChanged = nameof(OnSettingsChanged))] public LobbySettings Settings { get; set; }
        [Networked(OnChanged = nameof(OnCurrentGameModeChanged))] public GameModeBase CurrentGameMode { get; set; }
        [Networked, Capacity(4)] public NetworkLinkedList<CustomSceneRef> currentLoadedScenes { get; }

        public ClientContentLoaderService clientContentLoaderService;
        public ClientMapLoaderService clientMapLoaderService;
        public GameManager gameManager;
        public ContentManager contentManager;

        private static void OnSettingsChanged(Changed<LobbyManager> changed)
        {
            OnLobbySettingsChanged?.Invoke();
        }

        private static void OnCurrentGameModeChanged(Changed<LobbyManager> changed)
        {
            OnCurrentGamemodeChanged?.Invoke();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;
            gameManager = GameManager.singleton;
            contentManager = gameManager.contentManager;
            settings = new LobbySettingsManager(this);
        }

        public override void Spawned()
        {
            Settings = new LobbySettings(){ maxPlayersPerClient = 4, teams = 1 };
            currentLoadedScenes.Add(new CustomSceneRef() { source = 0, modIdentifier = 0, sceneIndex = 0 });
            Runner.SetActiveScene(1);
            OnLobbyManagerSpawned?.Invoke();
        }

        public void CallGamemodeSettingsChanged()
        {
            OnGamemodeSettingsChanged?.Invoke();
        }

        public async UniTask<bool> TryStartMatch()
        {
            Debug.Log("Trying to start match.");
            CleanupStrayReferences();
            if (Runner.IsServer == false)
            {
                Debug.LogError("START MATCH ERROR: Client trying to start match.");
                return false;
            }

            if (await settings.VerifyMatchSettings() == false)
            {
                Debug.LogError("START MATCH ERROR: Match settings invalid.");
                return false;
            }

            HashSet<ModObjectReference> fightersToLoad = new HashSet<ModObjectReference>();

            for (int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                for (int k = 0; k < ClientManager.clientManagers[i].ClientPlayers.Count; k++)
                {
                    for (int chara = 0;
                         chara < ClientManager.clientManagers[i].ClientPlayers[k].characterReferences.Count;
                         chara++)
                    {
                        if (!ClientManager.clientManagers[i].ClientPlayers[k].characterReferences[chara].IsValid())
                        {
                            Debug.LogError($"Player {i}:{k} has an invalid character reference.");
                            return false;
                        }   
                        
                        fightersToLoad.Add(ClientManager.clientManagers[i].ClientPlayers[k].characterReferences[chara]);
                    }
                }
            }

            foreach (var fighterStr in fightersToLoad)
            {
                List<PlayerRef> failedLoadPlayers =
                    await clientContentLoaderService.TellClientsToLoad<IFighterDefinition>(fighterStr);
                if (failedLoadPlayers == null || failedLoadPlayers.Count > 0)
                {
                    Debug.LogError($"START MATCH ERROR: Player failed to load fighter.");
                    return false;
                }
            }

            Debug.Log("Starting gamemode.");
            CurrentGameMode.StartGamemode();
            return true;
        }

        public void CleanupStrayReferences()
        {
            bool unload = true;
            List<(Type, ModObjectReference)> contentToUnload = new List<(Type, ModObjectReference)>();
            foreach (var v in contentManager.currentlyLoadedContent)
            {
                foreach (var b in v.Value)
                {
                    unload = true;
                    
                    if (typeof(IFighterDefinition).IsAssignableFrom(v.Key))
                    {
                        foreach (var cm in ClientManager.clientManagers)
                        {
                            foreach (var clientPlayer in cm.ClientPlayers)
                            {
                                foreach (var playerChara in clientPlayer.characterReferences)
                                {
                                    if (playerChara == b) unload = false;
                                }
                            }
                        }
                    }
                    
                    if (CurrentGameMode != null)
                    {
                        if (typeof(IGameModeDefinition).IsAssignableFrom(v.Key)) 
                            if (Settings.gamemodeReference == b) 
                                unload = false;
                        
                        if (CurrentGameMode.VerifyReference(b)) unload = false;
                    }
                    
                    if(unload) contentToUnload.Add((v.Key, b));
                }
            }
            
            foreach(var contentReference in contentToUnload) 
                contentManager.UnloadContentDefinition(contentReference.Item1, contentReference.Item2);
        }
    }
}