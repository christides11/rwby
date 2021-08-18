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

        public virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public override void Spawned()
        {
            base.Spawned();
            singleton = this;
            if (Object.HasStateAuthority)
            {
                MatchManager.onMatchInitialized += StartGamemode;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            if (Object.HasStateAuthority)
            {
                MatchManager.onMatchInitialized -= StartGamemode;
            }
        }

        public static void GamemodeStateChanged(Changed<GameModeBase> changed)
        {
            changed.Behaviour.GamemodeStateChanged();
        }

        public virtual void GamemodeStateChanged()
        {
            OnGamemodeStateChanged?.Invoke();
        }

        public virtual async UniTask<bool> SetupGamemode(ModObjectReference[] componentReferences, List<ModObjectReference> content)
        {
            foreach (ClientManager cm in ClientManager.clientManagers)
            {
                ModObjectReference characterReference = new ModObjectReference(cm.SelectedCharacter);
                bool cLoadResult = await ContentManager.instance.LoadContentDefinition(ContentType.Fighter, characterReference);
                if(cLoadResult == false)
                {
                    OnSetupFailure?.Invoke();
                    return false;
                }

                IFighterDefinition fighterDefinition = (IFighterDefinition)ContentManager.instance.GetContentDefinition(ContentType.Fighter, characterReference);

                bool fighterLoadResult = await fighterDefinition.LoadFighter();
                if(fighterLoadResult == false)
                {
                    OnSetupFailure?.Invoke();
                    return false;
                }
            }

            for (int i = 0; i < componentReferences.Length; i++)
            {
                bool cResult = await ContentManager.instance.LoadContentDefinition(ContentType.GamemodeComponent, componentReferences[i]);
                if(cResult == false)
                {
                    OnSetupFailure?.Invoke();
                    return false;
                }
            }

            OnSetupSuccess?.Invoke();
            return true;
        }

        protected void SetupFailed()
        {
            OnSetupFailure?.Invoke();
        }

        protected void SetupSuccess()
        {
            OnSetupSuccess?.Invoke();
        }

        public virtual async UniTask BreakdownGamemode()
        {

        }

        public virtual void StartGamemode()
        {
            GamemodeState = GameModeState.PRE_GAMEMODE;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
        }
    }
}