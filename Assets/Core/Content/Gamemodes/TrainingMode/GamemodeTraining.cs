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

        // TODO: Spawn player fighters.
        List<List<GameObject>> spawnPoints = new List<List<GameObject>>();
        List<int> spawnPointsCurr = new List<int>();
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

            SpawnPointHolder[] spawnPointHolders = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<SpawnPointHolder>();
            
            spawnPoints.Add(new List<GameObject>());
            spawnPointsCurr.Add(0);
            foreach (SpawnPointHolder sph in spawnPointHolders)
            {
                if (sph.singleTeamOnly)
                {
                    spawnPoints.Add(sph.spawnPoints);
                    spawnPointsCurr.Add(0);
                }
                else
                {
                    spawnPoints[0].AddRange(sph.spawnPoints);
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
                    spawnPointsCurr[clientPlayers[j].team]++;
                    
                }
            }

            RPC_SetupClientPlayers();
        }

        private Vector3 GetSpawnPosition(SessionGamemodePlayerDefinition clientPlayer)
        {
            return spawnPoints[clientPlayer.team][spawnPointsCurr[clientPlayer.team] % spawnPoints[clientPlayer.team].Count].transform.position;
        }

        protected override async UniTask SetupClientPlayerCharacters(SessionGamemodeClientContainer clientInfo, int playerIndex)
        {
            ClientManager cm = Runner.GetPlayerObject(clientInfo.clientRef).GetBehaviour<ClientManager>();
            NetworkObject no = null;
            PlayerCamera c = Runner.InstantiateInRunnerScene(GameManager.singleton.settings.playerCameraPrefab, Vector3.zero,
                Quaternion.identity);

            await UniTask.WaitUntil(() => Runner.TryFindObject(clientInfo.players[playerIndex].characterNetworkObjects[0], out no));
                
            Runner.AddSimulationBehaviour(c, null);
            c.Initialize(cm, playerIndex);
            c.SetLookAtTarget(no.GetComponent<FighterManager>());
            GameManager.singleton.localPlayerManager.SetPlayerCamera(playerIndex, c.Cam);
        }
        
        protected override async UniTask SetupClientPlayerHUD(SessionGamemodeClientContainer clientInfo, int playerIndex)
        {
            NetworkObject no = null;
            await UniTask.WaitUntil(() => Runner.TryFindObject(clientInfo.players[playerIndex].characterNetworkObjects[0], out no));
            PlayerCamera c = GameManager.singleton.localPlayerManager.GetPlayer(playerIndex).camera.GetComponent<PlayerCamera>();
            
            BaseHUD playerHUD = GameObject.Instantiate(GameManager.singleton.settings.baseUI);
            playerHUD.canvas.worldCamera = c.Cam;
            playerHUD.playerFighter = no.GetComponent<FighterManager>();
            Runner.AddSimulationBehaviour(playerHUD, null);
            
            await GameManager.singleton.contentManager.LoadContentDefinition(hudBankContentReference);
            
            IHUDElementbankDefinition HUDElementbank = GameManager.singleton.contentManager.GetContentDefinition<IHUDElementbankDefinition>(hudBankContentReference);
                
            await HUDElementbank.Load();
            
            var debugInfo = GameObject.Instantiate(HUDElementbank.GetHUDElement("debug"), playerHUD.transform, false);
            playerHUD.AddHUDElement(debugInfo.GetComponent<HUDElement>());
            var healthbar = GameObject.Instantiate(HUDElementbank.GetHUDElement("healthbar"), playerHUD.transform, false);
            playerHUD.AddHUDElement(healthbar.GetComponent<HUDElement>());
        }
    }
}