using Cysharp.Threading.Tasks;
using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class GameModeBase : NetworkBehaviour 
    {
        public delegate void EmptyAction();
        public delegate void GamemodeStateAction();
        public static event EmptyAction OnSetupSuccess;
        public static event EmptyAction OnSetupFailure;
        public static event GamemodeStateAction OnGamemodeStateChanged;

        public static GameModeBase singleton;

        [Networked(OnChanged = nameof(GamemodeStateChanged))] public GameModeState GamemodeState { get; set; }

        public static void GamemodeStateChanged(Changed<GameModeBase> changed)
        {
            changed.Behaviour.GamemodeStateChanged();
        }

        public virtual void GamemodeStateChanged()
        {
            OnGamemodeStateChanged?.Invoke();
        }

        public virtual async UniTask<bool> Load()
        {
            return true;
        }

        public virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public override void Spawned()
        {
            singleton = this;
            if (Object.HasStateAuthority)
            {

            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            if (Object.HasStateAuthority)
            {

            }
        }

        public virtual void AddGamemodeSettings(menus.LobbyMenu lobbyManager)
        {

        }

        public virtual async UniTask<bool> VerifyGameModeSettings()
        {
            return true;
        }

        public virtual void StartGamemode()
        {

        }

        public override void Render()
        {

        }

        public override void FixedUpdateNetwork()
        {

        }
    }
}