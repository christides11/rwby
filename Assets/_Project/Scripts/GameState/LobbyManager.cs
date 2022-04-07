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

        public ClientContentLoaderService clientContentLoaderService;
        public ClientMapLoaderService clientMapLoaderService;

        [Networked(OnChanged = nameof(OnSettingsChanged))]
        public LobbySettings Settings { get; set; }

        [Networked(OnChanged = nameof(OnCurrentGameModeChanged))] public GameModeBase CurrentGameMode { get; set; }
        [Networked, Capacity(4)] public NetworkLinkedList<CustomSceneRef> currentLoadedScenes { get; }

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

        public async UniTask<bool> TrySetGamemode(ModObjectReference gamemodeReference)
        {
            if (Object.HasStateAuthority == false) return false;

            List<PlayerRef> failedLoadPlayers =
                await clientContentLoaderService.TellClientsToLoad<IGameModeDefinition>(gamemodeReference);
            if (failedLoadPlayers == null)
            {
                Debug.LogError("Set Gamemode Local Failure");
                return false;
            }

            foreach (var v in failedLoadPlayers)
            {
                Debug.Log($"{v.PlayerId} failed to load {gamemodeReference.ToString()}.");
            }

            if (CurrentGameMode != null)
            {
                Runner.Despawn(CurrentGameMode.GetComponent<NetworkObject>());
            }

            for (int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                var tempList = ClientManager.clientManagers[i].ClientPlayers;
                for (int k = 0; k < tempList.Count; k++)
                {
                    ClientPlayerDefinition clientPlayerDef = tempList[k];
                    clientPlayerDef.team = 0;
                    tempList[k] = clientPlayerDef;
                }
            }

            IGameModeDefinition gamemodeDefinition =
                ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(gamemodeReference);
            GameObject gamemodePrefab = gamemodeDefinition.GetGamemode();
            CurrentGameMode = Runner.Spawn(gamemodePrefab.GetComponent<GameModeBase>(), Vector3.zero, Quaternion.identity);

            LobbySettings temp = Settings;
            temp.gamemodeReference = gamemodeReference;
            temp.teams = (byte)gamemodeDefinition.maximumTeams;
            Settings = temp;
            return true;
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

            if (await VerifyMatchSettings() == false)
            {
                Debug.LogError("START MATCH ERROR: Match settings invalid.");
                return false;
            }

            HashSet<ModObjectReference> fightersToLoad = new HashSet<ModObjectReference>();

            for (int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                for (int k = 0; k < ClientManager.clientManagers[i].ClientPlayers.Count; k++)
                {
                    if (!ClientManager.clientManagers[i].ClientPlayers[k].characterReference.IsValid())
                    {
                        Debug.LogError($"Player {i}:{k} has an invalid character reference.");
                        return false;
                    }

                    fightersToLoad.Add(ClientManager.clientManagers[i].ClientPlayers[k].characterReference);
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

        public async UniTask<bool> VerifyMatchSettings()
        {
            if (CurrentGameMode == null) return false;

            IGameModeDefinition gamemodeDefinition =
                ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(Settings.gamemodeReference);
            if (VerifyTeams(gamemodeDefinition) == false) return false;
            if (await CurrentGameMode.VerifyGameModeSettings() == false) return false;
            return true;
        }

        private bool VerifyTeams(IGameModeDefinition gamemodeDefiniton)
        {
            int[] teamCount = new int[Settings.teams];

            for (int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                for (int k = 0; k < ClientManager.clientManagers[i].ClientPlayers.Count; k++)
                {
                    byte playerTeam = ClientManager.clientManagers[i].ClientPlayers[k].team;
                    if (playerTeam == 0) return false;
                    teamCount[playerTeam - 1]++;
                }
            }

            for (int w = 0; w < teamCount.Length; w++)
            {
                if (teamCount[w] > gamemodeDefiniton.teams[w].maximumPlayers
                    || teamCount[w] < gamemodeDefiniton.teams[w].minimumPlayers) return false;
            }

            return true;
        }

        public TeamDefinition GetTeamDefinition(int team)
        {
            if (CurrentGameMode == null) return new TeamDefinition();
            if (team < 0 || team >= Settings.teams) return new TeamDefinition();
            return CurrentGameMode.definition.teams[team];
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
                                if (clientPlayer.characterReference == b) unload = false;
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