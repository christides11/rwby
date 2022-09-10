using Cysharp.Threading.Tasks;
using Fusion;
using Rewired.Integration.UnityUI;
using rwby.ui.mainmenu;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby.core.training
{
    public class GamemodeTraining : GameModeBase
    {

        public TrainingSettingsMenu settingsMenu;

        [Networked] public NetworkModObjectGUIDReference Map { get; set; }
        public ModGUIDContentReference localMap;
        
        [FormerlySerializedAs("hudBankReference")] [FormerlySerializedAs("testReference")] public ModGUIDContentReference hudBankContentReference;

        public TrainingCPUHandler cpuHandler;

        public override void Awake()
        {
            base.Awake();
            settingsMenu.gameObject.SetActive(false);
        }

        public override void Spawned()
        {
            base.Spawned();
            
            if (Object.HasStateAuthority)
            {
                PauseMenu.onOpened += OnPaused;
            }
        }

        private void OnPaused()
        {
            PauseMenu.singleton.AddOption("Training Options", OpenSettingsMenu);
        }

        private void OpenSettingsMenu(PlayerPointerEventData arg0)
        {
            PauseMenu.singleton.currentSubmenu = settingsMenu;
            settingsMenu.Open();
        }

        #region Lobby

        public override void SetGamemodeSettings(GameModeBase gamemode)
        {
            GamemodeTraining train = gamemode as GamemodeTraining;
            Map = train.localMap;
        }

        public override void AddGamemodeSettings(int player, LobbySettingsMenu settingsMenu, bool local = false)
        {
            ModGUIDContentReference mapRef = local ? localMap : Map;
            
            IMapDefinition mapDefinition = ContentManager.singleton.GetContentDefinition<IMapDefinition>(mapRef);
            string mapName = mapDefinition != null ? mapDefinition.Name : "None";
            settingsMenu.AddOption("Map", mapName).onSubmit.AddListener(async () => { await OpenMapSelection(player, local); });
        }

        private async UniTask OpenMapSelection(int player, bool local = false)
        {
            await ContentSelect.singleton.OpenMenu(player, (int)ContentType.Map,(a, b) =>
            {
                ContentSelect.singleton.CloseMenu(player);
                if (local)
                {
                    localMap = b;
                    WhenGamemodeSettingsChanged(true);
                }
                else
                {
                    Map = b;
                    WhenGamemodeSettingsChanged();
                }
            });
        }
        #endregion

        public override async UniTask<bool> VerifyGameModeSettings()
        {
            if (Runner.IsRunning == false) return true;
            List<PlayerRef> failedLoadPlayers = await sessionManager.clientContentLoaderService.TellClientsToLoad<IMapDefinition>(Map);
            if (failedLoadPlayers == null)
            {
                Debug.LogError("Load Map Local Failure");
                return false;
            }

            foreach (var v in failedLoadPlayers)
            {
                Debug.Log($"{v.PlayerId} failed to load {Map.ToString()}.");
            }

            if (failedLoadPlayers.Count != 0) return false;

            return true;
        }

        public override bool VerifyReference(ModGUIDContentReference contentReference)
        {
            if (contentReference == (ModGUIDContentReference)Map) return true;
            return false;
        }
        
        List<List<GameObject>> startingPoints = new List<List<GameObject>>();
        List<int> startingPointCurr = new List<int>();
        private List<GameObject> respawnPoints = new List<GameObject>();
        
        public override async UniTaskVoid StartGamemode()
        {
            Debug.Log("Attempting to start.");
            sessionManager.SessionState = SessionGamemodeStateType.LOADING_GAMEMODE;
            GamemodeState = GameModeState.INITIALIZING;

            IMapDefinition mapDefinition = ContentManager.singleton.GetContentDefinition<IMapDefinition>(Map);
            
            
            sessionManager.currentLoadedScenes.Clear();
            sessionManager.currentLoadedScenes.Add(new CustomSceneRef()
            {
                mapContentReference = Map,
                sceneIdentifier = 0
            });
            Runner.SetActiveScene(Runner.CurrentScene+1);

            await UniTask.WaitForEndOfFrame(); 
            var sh = sessionManager.gameManager.networkManager.GetSessionHandlerByRunner(Runner);
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
            sessionManager.SessionState = SessionGamemodeStateType.IN_GAMEMODE;

            StartingPointGroup[] spawnPointGroups = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<StartingPointGroup>();
            
            startingPoints.Add(new List<GameObject>());
            startingPointCurr.Add(0);
            foreach (StartingPointGroup sph in spawnPointGroups)
            {
                if (sph.forTeam)
                {
                    startingPoints.Add(sph.points);
                    startingPointCurr.Add(0);
                }
                else
                {
                    startingPoints[0].AddRange(sph.points);
                }
            }
            
            RespawnPointGroup[] respawnPointGroups = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<RespawnPointGroup>();

            foreach (RespawnPointGroup rpg in respawnPointGroups)
            {
                respawnPoints.AddRange(rpg.points);
            }
            
            MapHandler[] mapHandler = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<MapHandler>();

            if (mapHandler.Length > 0)
            {
                for (int i = 0; i < mapHandler.Length; i++)
                {
                    await mapHandler[i].Initialize(this);
                }
            }

            var clientDefinitions = sessionManager.ClientDefinitions;
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
                    NetworkObject no = Runner.Spawn(fighterDefinition.GetFighter().GetComponent<NetworkObject>(), GetSpawnPosition(playerTemp), Quaternion.identity, clientDefinitions[i].clientRef,
                        (a, b) =>
                        {
                            b.gameObject.name = $"{temp.clientRef.PlayerId}.{j} : {fighterDefinition.name}";
                            var fManager = b.GetBehaviour<FighterManager>();
                            b.GetBehaviour<FighterCombatManager>().Team = playerTemp.team;
                            b.GetBehaviour<FighterInputManager>().inputProvider = cm;
                            b.GetBehaviour<FighterInputManager>().inputSourceIndex = (byte)playerID;
                            b.GetBehaviour<FighterInputManager>().inputEnabled = true;
                            fManager.HealthManager.Health = fManager.fighterDefinition.Health;
                            var list = playerTemp.characterNetworkObjects;
                            list.Set(0, b.Id);
                            noClientPlayers.Set(playerID, playerTemp);
                            noClientDefinitions.Set(clientID, temp);
                        });
                    startingPointCurr[clientPlayers[j].team]++;
                    
                }
            }

            RPC_SetupClientPlayers();

            GamemodeState = GameModeState.PRE_MATCH;

            if (mapHandler.Length > 0)
            {
                for (int i = 0; i < mapHandler.Length; i++)
                {
                    await mapHandler[i].DoPreMatch(this);
                }
            }

            RPC_TransitionToMatch();

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

        private Vector3 GetSpawnPosition(SessionGamemodePlayerDefinition clientPlayer)
        {
            return startingPoints[clientPlayer.team][startingPointCurr[clientPlayer.team] % startingPoints[clientPlayer.team].Count].transform.position;
        }
        
        protected override async UniTask SetupClientPlayerCharacters(SessionGamemodeClientContainer clientInfo, int playerIndex)
        {
            ClientManager cm = Runner.GetPlayerObject(clientInfo.clientRef).GetBehaviour<ClientManager>();
            NetworkObject no = null;

            await UniTask.WaitUntil(() => Runner.TryFindObject(clientInfo.players[playerIndex].characterNetworkObjects[0], out no));

            var dummyCamera = Runner.InstantiateInRunnerScene(GameManager.singleton.settings.dummyCamera, Vector3.up,
                Quaternion.identity);
            var cameraSwitcher = Runner.InstantiateInRunnerScene(GameManager.singleton.settings.cameraSwitcher,
                Vector3.zero, Quaternion.identity);
            var lockonCameraManager =
                Runner.InstantiateInRunnerScene(GameManager.singleton.settings.lockonCameraManager, Vector3.zero,
                    Quaternion.identity);
            Runner.AddSimulationBehaviour(dummyCamera, null);
            Runner.AddSimulationBehaviour(cameraSwitcher, null);
            Runner.AddSimulationBehaviour(lockonCameraManager, null);

            dummyCamera.Initialize();
            cameraSwitcher.cam = dummyCamera;
            cameraSwitcher.RegisterCamera(0, lockonCameraManager);
            lockonCameraManager.Initialize(cameraSwitcher);
            cameraSwitcher.SetTarget(no.GetComponent<FighterManager>());
            cameraSwitcher.AssignControlTo(cm, playerIndex);
            cameraSwitcher.Disable();
            
            GameManager.singleton.localPlayerManager.SetPlayerCameraHandler(playerIndex, cameraSwitcher);
            GameManager.singleton.localPlayerManager.SetPlayerCamera(playerIndex, dummyCamera.camera);
        }
        
        protected override async UniTask SetupClientPlayerHUD(SessionGamemodeClientContainer clientInfo, int playerIndex)
        {
            ClientManager cm = Runner.GetPlayerObject(clientInfo.clientRef).GetBehaviour<ClientManager>();
            
            NetworkObject no = null;
            await UniTask.WaitUntil(() => Runner.TryFindObject(clientInfo.players[playerIndex].characterNetworkObjects[0], out no));
            FighterManager fm = no.GetComponent<FighterManager>();
            CameraSwitcher cameraHandler =
                GameManager.singleton.localPlayerManager.GetPlayer(playerIndex).cameraHandler;
            Camera c = GameManager.singleton.localPlayerManager.GetPlayer(playerIndex).camera;
            
            BaseHUD baseHUD = GameObject.Instantiate(GameManager.singleton.settings.baseUI);
            baseHUD.SetClient(cm, playerIndex);
            baseHUD.canvas.worldCamera = c;
            baseHUD.playerFighter = fm;
            baseHUD.cameraSwitcher = cameraHandler;
            Runner.AddSimulationBehaviour(baseHUD, null);
            
            GameManager.singleton.localPlayerManager.SetPlayerHUD(playerIndex, baseHUD);
            
            await GameManager.singleton.contentManager.LoadContentDefinition(hudBankContentReference);
            
            IHUDElementbankDefinition HUDElementbank = GameManager.singleton.contentManager.GetContentDefinition<IHUDElementbankDefinition>(hudBankContentReference);
                
            await HUDElementbank.Load();
            
            var debugInfo = GameObject.Instantiate(HUDElementbank.GetHUDElement("debug"), baseHUD.transform, false);
            baseHUD.AddHUDElement(debugInfo.GetComponent<HUDElement>());
            var pHUD = GameObject.Instantiate(HUDElementbank.GetHUDElement("phud"), baseHUD.transform, false);
            baseHUD.AddHUDElement(pHUD.GetComponent<HUDElement>());
            var worldHUD = GameObject.Instantiate(HUDElementbank.GetHUDElement("worldhud"), baseHUD.transform, false);
            baseHUD.AddHUDElement(worldHUD.GetComponent<HUDElement>());

            foreach (var hbank in fm.fighterDefinition.huds)
            {
                var convertedRef = new ModContentGUIDReference()
                {
                    contentGUID = hbank.contentReference.contentGUID,
                    contentType = (int)ContentType.HUDElementbank,
                    modGUID = hbank.contentReference.modGUID
                };
                var lResult = await GameManager.singleton.contentManager.LoadContentDefinition(GameManager.singleton.contentManager.ConvertModContentGUIDReference(convertedRef));

                if (!lResult)
                {
                    Debug.LogError("Error loading HUDbank.");
                    continue;
                }
                
                var hebank = GameManager.singleton.contentManager.GetContentDefinition<IHUDElementbankDefinition>(GameManager.singleton.contentManager.ConvertModContentGUIDReference(convertedRef));
                
                var hEle = GameObject.Instantiate(hebank.GetHUDElement(hbank.item), baseHUD.transform, false);
                baseHUD.AddHUDElement(pHUD.GetComponent<HUDElement>());
            }
        }
    }
}