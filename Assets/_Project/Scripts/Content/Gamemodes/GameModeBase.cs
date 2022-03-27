using Cysharp.Threading.Tasks;
using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class GameModeBase : NetworkBehaviour 
    {
        public delegate void EmptyAction();
        public delegate void GamemodeStateAction(GameModeBase gamemode);
        public static event EmptyAction OnSetupSuccess;
        public static event EmptyAction OnSetupFailure;
        public static event GamemodeStateAction OnGamemodeStateChanged;

        public static GameModeBase singleton;

        public IGameModeDefinition definition;
        
        [Networked(OnChanged = nameof(GamemodeStateChanged))] public GameModeState GamemodeState { get; set; }

        public static void GamemodeStateChanged(Changed<GameModeBase> changed)
        {
            changed.Behaviour.GamemodeStateChanged();
        }

        public virtual void GamemodeStateChanged()
        {
            OnGamemodeStateChanged?.Invoke(this);
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

        public virtual void AddGamemodeSettings(menus.LobbyMenuHandler lobbyManager)
        {

        }

        public virtual async UniTask<bool> VerifyGameModeSettings()
        {
            return true;
        }

        public virtual bool VerifyReference(ModObjectReference reference)
        {
            return false;
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