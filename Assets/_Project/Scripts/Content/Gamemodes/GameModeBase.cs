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

        public event EmptyAction OnLocalGamemodeSettingsChanged;
        public event EmptyAction OnGamemodeSettingsChanged;
        public static event EmptyAction OnSetupSuccess;
        public static event EmptyAction OnSetupFailure;
        public static event GamemodeStateAction OnGamemodeStateChanged;

        public static GameModeBase singleton;

        public IGameModeDefinition definition;
        
        [Networked(OnChanged = nameof(GamemodeStateChanged))] public GameModeState GamemodeState { get; set; }
        
        [Networked] public SessionManagerGamemode sessionManager { get; set; }

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
            
        }

        public override void Spawned()
        {
            singleton = this;
            if (Object.HasStateAuthority)
            {

            }
            DontDestroyOnLoad(gameObject);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            if (Object.HasStateAuthority)
            {

            }
        }
        
        public virtual void WhenGamemodeSettingsChanged(bool local = false)
        {
            if (local)
            {
                OnLocalGamemodeSettingsChanged?.Invoke();
            }
            else
            {
                OnGamemodeSettingsChanged?.Invoke();
            }
        }

        public virtual void SetGamemodeSettings(GameModeBase gamemode)
        {
            
        }
        
        public virtual void AddGamemodeSettings(int player, LobbySettingsMenu settingsMenu, bool local = false)
        {
            
        }

        public virtual async UniTask<bool> VerifyGameModeSettings()
        {
            return true;
        }

        public virtual bool VerifyReference(ModObjectGUIDReference reference)
        {
            return false;
        }

        public virtual async UniTaskVoid StartGamemode()
        {

        }

        public override void Render()
        {

        }

        public override void FixedUpdateNetwork()
        {

        }

        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public virtual async void RPC_SetupClientPlayers()
        {
            if (!Runner.LocalPlayer.IsValid) return;
            var cInfo = sessionManager.GetClientInfo(Runner.LocalPlayer);

            await SetupClientPlayers(cInfo);
        }

        protected virtual async UniTask SetupClientPlayers(SessionGamemodeClientContainer cInfo)
        {
            NetworkObject no = null;
            for (int i = 0; i < cInfo.players.Count; i++)
            {
                if (cInfo.players[i].characterNetworkObjects.Count < 1) return;

                PlayerCamera c = GameObject.Instantiate(GameManager.singleton.settings.playerCameraPrefab, Vector3.zero,
                    Quaternion.identity);
                
                int playerID = i;
                await UniTask.WaitUntil(() => Runner.TryFindObject(cInfo.players[playerID].characterNetworkObjects[0], out no));
                
                c.SetLookAtTarget(no.GetComponent<FighterManager>());
                GameManager.singleton.localPlayerManager.SetPlayerCamera(i, c.Cam);
            }
        }
    }
}