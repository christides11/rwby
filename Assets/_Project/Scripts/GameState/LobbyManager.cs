using Cysharp.Threading.Tasks;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class LobbyManager : NetworkBehaviour
    {
        public delegate void EmptyAction();
        public static event EmptyAction OnLobbyPlayersUpdated;
        public static event EmptyAction OnLobbySettingsChanged;
        public static event EmptyAction OnCurrentGamemodeChanged;

        public static LobbyManager singleton;

        public ClientContentLoaderService clientContentLoaderService;

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
            //UpdateGamemodeOptions();
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

            GameObject gamemodePrefab = ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(gamemodeReference).GetGamemode();
            GameModeBase gamemodeObj = Runner.Spawn(gamemodePrefab.GetComponent<GameModeBase>(), Vector3.zero, Quaternion.identity);
            CurrentGameMode = gamemodeObj;

            LobbySettings temp = Settings;
            temp.gamemodeReference = gamemodeReference.ToString();
            Settings = temp;
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