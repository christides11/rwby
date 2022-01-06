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
        public static event EmptyAction OnLobbyPlayersUpdated;
        public static event EmptyAction OnLobbySettingsChanged;
        public static event EmptyAction OnCurrentGamemodeChanged;
        public static event EmptyAction OnGamemodeSettingsChanged;

        public static LobbyManager singleton;

        public ClientContentLoaderService clientContentLoaderService;
        public ClientMapLoaderService clientMapLoaderService;

        [Networked] public int maxPlayersPerClient { get; set; } = 4;

        [Networked(OnChanged = nameof(OnSettingsChanged))] public LobbySettings Settings { get; set; }
        [Networked(OnChanged = nameof(OnCurrentGameModeChanged))] public GameModeBase CurrentGameMode { get; set; }

        [Networked] private NetworkRNG MainRNGGenerator { get; set; }

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
        }

        public override void Spawned()
        {

        }

        public void CallGamemodeSettingsChanged()
        {
            OnGamemodeSettingsChanged?.Invoke();
        }

        public async UniTask TrySetGamemode(ModObjectReference gamemodeReference)
        {
            if (Object.HasStateAuthority == false) return;

            List<PlayerRef> failedLoadPlayers = await clientContentLoaderService.TellClientsToLoad<IGameModeDefinition>(gamemodeReference);
            if (failedLoadPlayers == null)
            {
                Debug.LogError("Set Gamemode Local Failure");
                return;
            }

            foreach (var v in failedLoadPlayers)
            {
                Debug.Log($"{v.PlayerId} failed to load {gamemodeReference.ToString()}.");
            }

            if(CurrentGameMode != null)
            {
                Runner.Despawn(CurrentGameMode.GetComponent<NetworkObject>());
            }

            for(int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                var tempList = ClientManager.clientManagers[i].ClientPlayers;
                for (int k = 0; k < tempList.Count; k++)
                {
                    ClientPlayerDefinition clientPlayerDef = tempList[k];
                    clientPlayerDef.team = 0;
                    tempList[k] = clientPlayerDef;
                }
            }

            IGameModeDefinition gamemodeDefinition = ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(gamemodeReference);
            GameObject gamemodePrefab = gamemodeDefinition.GetGamemode();
            GameModeBase gamemodeObj = Runner.Spawn(gamemodePrefab.GetComponent<GameModeBase>(), Vector3.zero, Quaternion.identity);
            CurrentGameMode = gamemodeObj;

            LobbySettings temp = Settings;
            temp.gamemodeReference = gamemodeReference.ToString();
            temp.teams = (byte)gamemodeDefinition.maximumTeams;
            Settings = temp;
        }

        public async UniTask<bool> TryStartMatch()
        {
            if (Runner.IsServer == false) return false;
            if (await VerifyMatchSettings() == false) return false;

            HashSet<string> fightersToLoad = new HashSet<string>();

            for(int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                for(int k = 0; k < ClientManager.clientManagers[i].ClientPlayers.Count; k++)
                {
                    if (ClientManager.clientManagers[i].ClientPlayers[k].characterReference.Length <= 1) return false;
                    fightersToLoad.Add(ClientManager.clientManagers[i].ClientPlayers[k].characterReference);
                }
            }

            foreach(string fighterStr in fightersToLoad)
            {
                List<PlayerRef> failedLoadPlayers = await clientContentLoaderService.TellClientsToLoad<IFighterDefinition>(new ModObjectReference(fighterStr));
                if (failedLoadPlayers == null || failedLoadPlayers.Count > 0) return false;
            }

            Debug.Log("Match Start Success");
            CurrentGameMode.StartGamemode();
            return true;
        }

        public async UniTask<bool> VerifyMatchSettings()
        {
            if (CurrentGameMode == null) return false;

            IGameModeDefinition gamemodeDefinition = ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(new ModObjectReference(Settings.gamemodeReference));
            if (VerifyTeams(gamemodeDefinition) == false) return false;
            if (await CurrentGameMode.VerifyGameModeSettings() == false) return false;
            return true;
        }

        private bool VerifyTeams(IGameModeDefinition gamemodeDefiniton)
        {
            if (Settings.teams == 0) return true;

            int[] teamCount = new int[Settings.teams];

            for(int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                for(int k = 0; k < ClientManager.clientManagers[i].ClientPlayers.Count; k++)
                {
                    byte playerTeam = ClientManager.clientManagers[i].ClientPlayers[k].team;
                    if (playerTeam == 0) return false;
                    teamCount[playerTeam - 1]++;
                }
            }

            for(int w = 0; w < teamCount.Length; w++)
            {
                if (teamCount[w] > gamemodeDefiniton.teams[w].maximumPlayers 
                    || teamCount[w] < gamemodeDefiniton.teams[w].minimumPlayers) return false;
            }

            return true;
        }

        public int GetIntRangeInclusive(int min, int max)
        {
            NetworkRNG tempRNG = MainRNGGenerator;
            int value = tempRNG.RangeInclusive(min, max);
            MainRNGGenerator = tempRNG;
            return value;
        }
    }
}