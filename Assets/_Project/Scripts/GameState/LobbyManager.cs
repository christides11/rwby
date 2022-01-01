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
        public static event EmptyAction OnCurrentGamemodeChanged;

        public static LobbyManager singleton;

        public ClientContentLoaderService clientContentLoaderService;

        [Networked] public int maxPlayersPerClient { get; set; } = 4;

        [Networked(OnChanged = nameof(OnCurrentGameModeChanged))] public GameModeBase CurrentGameMode { get; set; }

        [Networked] private NetworkRNG MainRNGGenerator { get; set; }

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