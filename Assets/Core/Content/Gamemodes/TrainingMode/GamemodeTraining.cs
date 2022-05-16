using Cysharp.Threading.Tasks;
using Fusion;
using Rewired.Integration.UnityUI;
using rwby.ui.mainmenu;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace rwby.core.training
{
    public struct CPUReference : INetworkStruct
    {
        public ModObjectReference characterReference;
        public NetworkId objectId;
    }
    
    public class GamemodeTraining : GameModeBase
    {
        public event EmptyAction OnCPUListUpdated;
        
        public TrainingSettingsMenu settingsMenu;

        [Networked(OnChanged = nameof(CpuListUpdated)), Capacity(4)] public NetworkLinkedList<CPUReference> cpus { get; }

        [Networked] public NetworkModObjectGUIDReference Map { get; set; }
        public ModObjectGUIDReference localMap;

        private static void CpuListUpdated(Changed<GamemodeTraining> changed)
        {
            changed.Behaviour.OnCPUListUpdated?.Invoke();
            _ = changed.Behaviour.CheckCPUList();
        }

        public override void Awake()
        {
            base.Awake();
            settingsMenu.gameObject.SetActive(false);
        }

        private async UniTask CheckCPUList()
        {
            /*
            if (Object.HasStateAuthority == false) return;

            for(int i = 0; i < cpus.Count; i++)
            {
                ModObjectReference objectReference = cpus[i].characterReference;
                if(objectReference.IsValid() && cpus[i].objectId.IsValid == false)
                {
                    List<PlayerRef> failedLoadPlayers = await SessionManagerClassic.singleton.clientContentLoaderService.TellClientsToLoad<IFighterDefinition>(objectReference);
                    if (failedLoadPlayers == null)
                    {
                        Debug.LogError($"Load CPU {objectReference} failure.");
                        continue;
                    }

                    int indexTemp = i;
                    IFighterDefinition fighterDefinition = ContentManager.singleton.GetContentDefinition<IFighterDefinition>(objectReference);
                    NetworkObject no = Runner.Spawn(fighterDefinition.GetFighter().GetComponent<NetworkObject>(), Vector3.up, Quaternion.identity, null,
                        (a, b) =>
                        {
                            b.gameObject.name = $"CPU.{b.Id} : {fighterDefinition.Name}";
                            b.GetBehaviour<FighterCombatManager>().Team = 0;
                            _ = b.GetBehaviour<FighterManager>().OnFighterLoaded();
                            var list = cpus;
                            CPUReference temp = list[indexTemp];
                            temp.objectId = b.Id;
                            list[indexTemp] = temp;
                        });
                }
            }*/
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
            ModObjectGUIDReference mapRef = local ? localMap : Map;
            
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

        public override bool VerifyReference(ModObjectGUIDReference reference)
        {
            if (reference == (ModObjectGUIDReference)Map) return true;
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
                mapReference = Map,
                sceneIdentifier = 0
            });
            Runner.SetActiveScene(5);

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
                            b.GetBehaviour<FighterCombatManager>().Team = playerTemp.team;
                            b.GetBehaviour<FighterInputManager>().inputProvider = cm;
                            b.GetBehaviour<FighterInputManager>().inputSourceIndex = (byte)playerID;
                            b.GetBehaviour<FighterInputManager>().inputEnabled = true;
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
    }
}