using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace rwby.core.versus
{

    public class GamemodeVersusInitialization : NetworkBehaviour, IGamemodeInitialization
    {
        public GamemodeVersus gamemode;
        
        public async UniTask<bool> VerifyGameModeSettings()
        {
            if (Runner.IsRunning == false) return true;
            List<PlayerRef> failedLoadPlayers = await gamemode.sessionManager.clientContentLoaderService.TellClientsToLoad<IMapDefinition>(gamemode.Map);
            if (failedLoadPlayers == null)
            {
                Debug.LogError("Load Map Local Failure");
                return false;
            }

            foreach (var v in failedLoadPlayers)
            {
                Debug.Log($"{v.PlayerId} failed to load {gamemode.Map.ToString()}.");
            }

            if (failedLoadPlayers.Count != 0) return false;

            return true;
        }
        
        public async UniTaskVoid StartGamemode()
        {
            gamemode.sessionManager.SessionState = SessionGamemodeStateType.LOADING_GAMEMODE;
            gamemode.GamemodeState = GameModeState.INITIALIZING;

            IMapDefinition mapDefinition = ContentManager.singleton.GetContentDefinition<IMapDefinition>(gamemode.Map);
            
            
            gamemode.sessionManager.currentLoadedScenes.Clear();
            gamemode.sessionManager.currentLoadedScenes.Add(new CustomSceneRef()
            {
                mapContentReference = gamemode.Map,
                sceneIdentifier = 0
            });
            Runner.SetActiveScene(Runner.CurrentScene+1);

            RPC_FadeOutMusic();
            
            await UniTask.WaitForEndOfFrame(); 
            var sh = gamemode.sessionManager.gameManager.networkManager.GetSessionHandlerByRunner(Runner);
            await UniTask.WaitUntil(() => sh.netSceneManager.loadPercentage == 100);
            
            int lowestLoadPercentage = 0;
            while (lowestLoadPercentage != 100)
            {
                lowestLoadPercentage = 100;
                foreach (var playerRef in Runner.ActivePlayers)
                {
                    ClientManager cm = Runner.GetPlayerObject(playerRef).GetBehaviour<ClientManager>();
                    if (cm.mapLoadPercent < lowestLoadPercentage) lowestLoadPercentage = cm.mapLoadPercent;
                }
                await UniTask.WaitForEndOfFrame();
            }
            
            await UniTask.WaitForEndOfFrame();
            gamemode.sessionManager.SessionState = SessionGamemodeStateType.IN_GAMEMODE;

            StartingPointGroup[] spawnPointGroups = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<StartingPointGroup>();
            
            gamemode.startingPoints.Add(new List<GameObject>());
            gamemode.startingPointCurr.Add(0);
            foreach (StartingPointGroup sph in spawnPointGroups)
            {
                if (sph.forTeam)
                {
                    gamemode.startingPoints.Add(sph.points);
                    gamemode.startingPointCurr.Add(0);
                }
                else
                {
                    gamemode.startingPoints[0].AddRange(sph.points);
                }
            }
            
            RespawnPointGroup[] respawnPointGroups = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<RespawnPointGroup>();

            foreach (RespawnPointGroup rpg in respawnPointGroups)
            {
                gamemode.respawnPoints.AddRange(rpg.points);
            }
            
            MapHandler[] mapHandler = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<MapHandler>();

            if (mapHandler.Length > 0)
            {
                for (int i = 0; i < mapHandler.Length; i++)
                {
                    await mapHandler[i].Initialize(gamemode);
                }
            }

            var clientDefinitions = gamemode.sessionManager.ClientDefinitions;
            for (int i = 0; i < clientDefinitions.Count; i++)
            {
                var temp = clientDefinitions[i];
                var clientPlayers = temp.players;
                for (int j = 0; j < clientPlayers.Count; j++)
                {
                    if (clientPlayers[j].characterReferences.Count == 0) continue;
                    int clientID = i;
                    int playerID = j;
                    var playerTemp = clientPlayers[j];
                    var playerCharacterRefs = playerTemp.characterReferences;
                    NetworkObject cm = Runner.GetPlayerObject(temp.clientRef);
                    
                    IFighterDefinition fighterDefinition = (IFighterDefinition)GameManager.singleton.contentManager.GetContentDefinition(playerCharacterRefs[0]);

                    var noClientDefinitions = clientDefinitions;
                    var noClientPlayers = clientPlayers;
                    var spawnPosition = gamemode.GetSpawnPosition(playerTemp.team);
                    NetworkObject no = Runner.Spawn(fighterDefinition.GetFighter().GetComponent<NetworkObject>(), spawnPosition.position, spawnPosition.rotation, clientDefinitions[i].clientRef,
                        (a, b) =>
                        {
                            b.gameObject.name = $"{temp.clientRef.PlayerId}.{j} : {fighterDefinition.name}";
                            var fManager = b.GetBehaviour<FighterManager>();
                            b.GetBehaviour<FighterCombatManager>().Team = playerTemp.team;
                            b.GetBehaviour<FighterInputManager>().inputProvider = cm;
                            b.GetBehaviour<FighterInputManager>().inputSourceIndex = (byte)playerID;
                            b.GetBehaviour<FighterInputManager>().inputEnabled = true;
                            b.GetBehaviour<FighterManager>().callbacks = gamemode.playerHandler;
                            fManager.HealthManager.Health = fManager.fighterDefinition.Health;
                            var list = playerTemp.characterNetworkObjects;
                            list.Set(0, b.Id);
                            noClientPlayers.Set(playerID, playerTemp);
                            noClientDefinitions.Set(clientID, temp);
                        });
                    gamemode.startingPointCurr[clientPlayers[j].team]++;
                    
                }
            }

            gamemode.RPC_SetupClientPlayers();

            gamemode.GamemodeState = GameModeState.PRE_MATCH;

            RPC_PlayMusic();
            
            if (mapHandler.Length > 0)
            {
                for (int i = 0; i < mapHandler.Length; i++)
                {
                    await mapHandler[i].DoPreMatch(gamemode);
                }
            }

            gamemode.TimeLimitTimer = TickTimer.CreateFromSeconds(Object.Runner, gamemode.TimeLimitMinutes * 60);
            RPC_TransitionToMatch();
        }
        
        
        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_FadeOutMusic()
        {
            GameManager.singleton.musicManager.FadeAll(1.0f);
        }

        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_PlayMusic()
        {
            if (gamemode.sessionManager.musicToPlay == null) return;
            
            GameManager.singleton.musicManager.Play(gamemode.sessionManager.musicToPlay.Song);
        }

        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_TransitionToMatch()
        {
            CameraSwitcher[] cameraSwitchers = GameObject.FindObjectsOfType<CameraSwitcher>();
            foreach (CameraSwitcher cs in cameraSwitchers)
            {
                cs.SwitchTo(0);
            }

            _ = TransitionToMatch();
        }

        public async UniTask TransitionToMatch()
        {
            /*
            IHUDElementbankDefinition HUDElementbank = GameManager.singleton.contentManager.GetContentDefinition<IHUDElementbankDefinition>(hudBankContentReference);

            foreach (var p in GameManager.singleton.localPlayerManager.localPlayers)
            {
                var baseHUD = p.hud;
                var introElement = GameObject.Instantiate(HUDElementbank.GetHUDElement("intro"), baseHUD.transform, false);
                baseHUD.AddHUDElement(introElement.GetComponent<HUDElement>());
            }

            int startFrame = Runner.Tick + (60 * 2);
            await UniTask.WaitUntil(() => Runner.Tick >= startFrame);*/
            GameModeBase.singleton.GamemodeState = GameModeState.MATCH_IN_PROGRESS;
        }
    }
}