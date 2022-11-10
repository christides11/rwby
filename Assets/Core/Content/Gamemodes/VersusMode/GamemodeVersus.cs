using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using rwby.core.training;
using rwby.Debugging;
using rwby.ui;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby.core.versus
{
    public class GamemodeVersus : GameModeBase, ITimeProvider
    {
        public override IGamemodeInitialization InitializationHandler => initialization;
        public override IGamemodeLobbyUI LobbyUIHandler => lobbyUI;
        public override IGamemodeTeardown TeardownHandler => teardown;
        [Networked] public NetworkModObjectGUIDReference Map { get; set; }
        [Networked] public int PointsRequired { get; set; } = 20;
        [Networked] public int TimeLimitMinutes { get; set; } = 10;
        public ModGUIDContentReference localMap;
        public int localPointsRequired = 20;
        public int localTimeLimitMinutes = 10;

        [Networked] public TickTimer TimeLimitTimer { get; set; }

        public ModGUIDContentReference hudBankContentReference;

        public GamemodeVersusInitialization initialization;
        public GamemodeVersusTeardown teardown;
        public GamemodeVersusLobbyUI lobbyUI;
        public VersusPlayerHandler playerHandler;

        public List<List<GameObject>> startingPoints = new List<List<GameObject>>();
        public List<int> startingPointCurr = new List<int>();
        public List<GameObject> respawnPoints = new List<GameObject>();

        public override void Render()
        {
            base.Render();
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            switch (GamemodeState)
            {
                case GameModeState.MATCH_IN_PROGRESS:
                    if (TimeLimitTimer.Expired(Runner))
                    {
                        EndMatch();
                    }
                    break;
            }
        }

        public void EndMatch()
        {
            GamemodeState = GameModeState.POST_MATCH;
            teardown.Teardown();
        }

        public override async UniTask SetGamemodeSettings(string args)
        {
            base.SetGamemodeSettings(args);
            var r = ConsoleReader.SplitInputLine(args);

            if (r.input.Length < 1)
            {
                Debug.LogError("Not enough args for gamemode settings.");
                return;
            }
            
            string[] gamemodeRefStr = r.input[0].Split(',');
            ModObjectSetContentReference mapSetReference = new ModObjectSetContentReference(gamemodeRefStr[0], gamemodeRefStr[1]);

            ModContentGUIDReference mapGUIDReference = new ModContentGUIDReference()
            {
                modGUID = mapSetReference.modGUID,
                contentType = (int)ContentType.Map,
                contentGUID = mapSetReference.contentGUID
            };
            var mapGUIDContentReference =
                ContentManager.singleton.ConvertModContentGUIDReference(mapGUIDReference);

            var loadResult = await ContentManager.singleton.LoadContentDefinition(mapGUIDContentReference);
            if (!loadResult)
            {
                Debug.LogError($"Error loading map: {mapGUIDReference.ToString()}");
                return;
            }

            Map = mapGUIDContentReference;
        }
        
        public override void SetGamemodeSettings(GameModeBase gamemode)
        {
            GamemodeVersus versus = gamemode as GamemodeVersus;
            Map = versus.localMap;
            TimeLimitMinutes = versus.localTimeLimitMinutes;
            PointsRequired = versus.localPointsRequired;
        }

        /*
        public override bool VerifyReference(ModGUIDContentReference contentReference)
        {
            if (contentReference == (ModGUIDContentReference)Map) return true;
            return false;
        }*/
        
        public Transform GetSpawnPosition(int team)
        {
            return startingPoints[team][startingPointCurr[team] % startingPoints[team].Count].transform;
        }

        public Transform GetRespawnPosition(int team)
        {
            var tempRngGenerator = rngGenerator;
            int val = tempRngGenerator.RangeExclusive(0, respawnPoints.Count);
            rngGenerator = tempRngGenerator;
            return respawnPoints[val].transform;
        }

        protected override async UniTask SetupClientPlayerCharacters(SessionGamemodeClientContainer clientInfo, int playerIndex)
        {
            ClientManager cm = Runner.GetPlayerObject(clientInfo.clientRef).GetBehaviour<ClientManager>();
            NetworkObject no = null;

            await UniTask.WaitUntil(() => Runner.TryFindObject(clientInfo.players[playerIndex].characterNetworkObjects[0], out no));

            FighterManager fm = no.GetComponent<FighterManager>();
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
            foreach (var c in fm.fighterDefinition.cameras)
            {
                var cc = Runner.InstantiateInRunnerScene(c.cam, Vector3.zero, Quaternion.identity);
                Runner.AddSimulationBehaviour(cc);
                cameraSwitcher.RegisterCamera(c.id, cc);
                cc.Initialize(cameraSwitcher);
            }
            cameraSwitcher.SetTarget(no.GetComponent<FighterManager>());
            cameraSwitcher.AssignControlTo(cm, playerIndex);
            cameraSwitcher.Disable();
            fm.OnCameraModeChanged += cameraSwitcher.WhenCameraModeChanged;
            
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
            
            var pHUD = GameObject.Instantiate(HUDElementbank.GetHUDElement("phud"), baseHUD.transform, false);
            baseHUD.AddHUDElement(pHUD.GetComponent<HUDElement>());
            var worldHUD = GameObject.Instantiate(HUDElementbank.GetHUDElement("worldhud"), baseHUD.transform, false);
            baseHUD.AddHUDElement(worldHUD.GetComponent<HUDElement>());
            var timerHUD = GameObject.Instantiate(HUDElementbank.GetHUDElement("timer"), baseHUD.transform, false);
            baseHUD.AddHUDElement(timerHUD.GetComponent<HUDElement>());

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

        public int GetTimeInSeconds()
        {
            return !TimeLimitTimer.IsRunning ? 0 : (int)TimeLimitTimer.RemainingTime(Runner);
        }
    }
}
