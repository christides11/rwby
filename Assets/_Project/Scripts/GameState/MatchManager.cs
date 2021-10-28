using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace rwby
{
    public enum MatchSettingsLoadFailedReason
    {
        INVALID_GAMEMODE,
        INVALID_CONTENT,
        INVALID_COMPONENT
    }

    public class MatchManager : NetworkBehaviour
    {
        public delegate void EmptyAction();
        public delegate void MatchSettingsFailedAction(MatchSettingsLoadFailedReason reason);
        public static event MatchSettingsFailedAction onMatchSettingsLoadFailed;
        public static event EmptyAction onMatchSettingsLoadSuccess;
        public static event EmptyAction onMatchInitialized;

        public static MatchManager instance;

        public bool initializingMatch = false;

        [Networked(OnChanged = nameof(OnMatchSettingsChanged)), Capacity(50)] public string SelectedGamemode { get; set; }

        [Networked(OnChanged = nameof(OnMatchSettingsChanged)), Capacity(120)] public string GamemodeContent { get; set; }

        [Networked] public NetworkRNG EffectRNGenerator { get; set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }

        public static void OnMatchSettingsChanged(Changed<MatchManager> changed)
        {
            changed.Behaviour.OnMatchSettingsChanged();
        }

        private void OnMatchSettingsChanged()
        {
            _ = CheckMatchSettings();
        }

        private async UniTask CheckMatchSettings()
        {
            Debug.Log("Checking match settings...");
            ModObjectReference gamemodeReference = new ModObjectReference(SelectedGamemode);
            bool gamemodeLoadResult = await ContentManager.instance.LoadContentDefinition(ContentType.Gamemode, gamemodeReference);

            if (gamemodeLoadResult == false)
            {
                onMatchSettingsLoadFailed?.Invoke(MatchSettingsLoadFailedReason.INVALID_GAMEMODE);
                return;
            }

            IGameModeDefinition gamemodeDef = (IGameModeDefinition)ContentManager.instance.GetContentDefinition(ContentType.Gamemode, gamemodeReference);

            string[] gamemodeContents = new string[0];
            if(string.IsNullOrEmpty(GamemodeContent) == false)
            {
                gamemodeContents = GamemodeContent.Split(',');
            }

            if (gamemodeContents.Length != gamemodeDef.ContentRequirements.Length)
            {
                Debug.LogError($"Invalid content length. {GamemodeContent.Length} vs {gamemodeDef.ContentRequirements.Length}");
                onMatchSettingsLoadFailed?.Invoke(MatchSettingsLoadFailedReason.INVALID_CONTENT);
                return;
            }

            bool gamemodeObjLoadResult = await gamemodeDef.LoadGamemode();
            if(gamemodeObjLoadResult == false)
            {
                onMatchSettingsLoadFailed?.Invoke(MatchSettingsLoadFailedReason.INVALID_GAMEMODE);
                return;
            }

            for(int i = 0; i < gamemodeContents.Length; i++)
            {
                bool contentLoadResult = await ContentManager.instance.LoadContentDefinition(gamemodeDef.ContentRequirements[i], new ModObjectReference(gamemodeContents[i]));
                if(contentLoadResult == false)
                {
                    Debug.LogError("Invalid loaded content.");
                    onMatchSettingsLoadFailed?.Invoke(MatchSettingsLoadFailedReason.INVALID_CONTENT);
                    return;
                }
            }

            // Check if we have the gamemode components.
            for(int w = 0; w < gamemodeDef.GameModeComponentReferences.Length; w++)
            {
                bool componentLoadResult = await ContentManager.instance.LoadContentDefinition(ContentType.GamemodeComponent, gamemodeDef.GameModeComponentReferences[w]);
                if(componentLoadResult == false)
                {
                    onMatchSettingsLoadFailed?.Invoke(MatchSettingsLoadFailedReason.INVALID_COMPONENT);
                    return;
                }
            }

            Debug.Log("Match setting success.");
            onMatchSettingsLoadSuccess?.Invoke();
        }

        public virtual async UniTask StartMatch()
        {
            if (Object.HasStateAuthority == false) return;

            if (initializingMatch == true)
            {
                return;
            }
            initializingMatch = true;

            // Disconnect players that aren't ready.
            foreach(var p in NetworkManager.singleton.FusionLauncher.Players)
            {
                ClientManager cm = p.Value.GetComponent<ClientManager>();
                if(cm.LobbyReadyStatus == false)
                {
                    NetworkManager.singleton.FusionLauncher.NetworkRunner.Disconnect(p.Key);
                }
            }

            // Spawn Gamemode
            IGameModeDefinition gamemodeDef = (IGameModeDefinition)ContentManager.instance.GetContentDefinition(ContentType.Gamemode, new ModObjectReference(SelectedGamemode));
            if(gamemodeDef == null)
            {
                initializingMatch = false;
                return;
            }

            bool gamemodeLoadResult = await gamemodeDef.LoadGamemode();
            if(gamemodeLoadResult == false)
            {
                initializingMatch = false;
                return;
            }

            List<ModObjectReference> refs = new List<ModObjectReference>();
            string[] sRefs = GamemodeContent.Split(',');
            for (int i = 0; i < sRefs.Length; i++)
            {
                refs.Add(new ModObjectReference(sRefs[i]));
            }

            
            GameModeBase gmb = NetworkManager.singleton.FusionLauncher.NetworkRunner.Spawn(gamemodeDef.GetGamemode().GetComponent<NetworkObject>(), Vector3.zero, Quaternion.identity)
                .GetComponent<GameModeBase>();
            bool gamemodeSetupResult = await gmb.SetupGamemode(gamemodeDef.GameModeComponentReferences, refs);

            if(gamemodeSetupResult == false)
            {
                initializingMatch = false;
                await gmb.BreakdownGamemode();
                NetworkManager.singleton.FusionLauncher.NetworkRunner.Despawn(gmb.GetComponent<NetworkObject>());
                return;
            }

            // Setup gamemode on all clients
            RPC_SetupGamemode();
            bool allReady = false;
            while (allReady == false)
            {
                allReady = true;
                foreach(var p in NetworkManager.singleton.FusionLauncher.Players)
                {
                    if(p.Value.GetComponent<ClientManager>().MatchReadyStatus == false)
                    {
                        allReady = false;
                    }
                }

                await UniTask.WaitForFixedUpdate();
            }

            
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(GameManager.singleton.currentMapSceneName));
            if (NetworkManager.singleton.FusionLauncher.TryGetSceneRef(out SceneRef scene))
            {
                if (NetworkManager.singleton.FusionLauncher.NetworkRunner.GameMode != GameMode.Client)
                    NetworkManager.singleton.FusionLauncher.NetworkRunner.SetActiveScene(scene);
            }
            EffectRNGenerator = new NetworkRNG(Runner.Simulation.Tick);
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            initializingMatch = false;
            Debug.Log("Setup complete.");
            onMatchInitialized?.Invoke();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        public void RPC_SetupGamemode()
        {
            IGameModeDefinition gamemodeDef = (IGameModeDefinition)ContentManager.instance.GetContentDefinition(ContentType.Gamemode, new ModObjectReference(SelectedGamemode));
            List<ModObjectReference> refs = new List<ModObjectReference>();
            string[] sRefs = GamemodeContent.Split(',');
            for (int i = 0; i < sRefs.Length; i++)
            {
                refs.Add(new ModObjectReference(sRefs[i]));
            }

            _ = GameModeBase.singleton.SetupGamemode(gamemodeDef.GameModeComponentReferences, refs);
        }

        public override void FixedUpdateNetwork()
        {
            // All of the match management logic happens server-side, so bail if we're not the server.
            if (Object.HasStateAuthority == false) return;
        }

        public int GetIntRangeInclusive(int min, int max)
        {
            NetworkRNG tempRNG = EffectRNGenerator;
            int value = tempRNG.RangeInclusive(min, max);
            EffectRNGenerator = tempRNG;
            return value;
        }
    }
}