using Cysharp.Threading.Tasks;
using Fusion;
using Rewired.Integration.UnityUI;
using rwby.menus;
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

        public ModObjectReference mapReference = new ModObjectReference();
        public TrainingSettingsMenu settingsMenu;

        [Networked(OnChanged = nameof(CpuListUpdated)), Capacity(4)] public NetworkLinkedList<CPUReference> cpus { get; }

        [Networked] public TrainingGamemodeSettings gamemodeSettings { get; set; }
        public TrainingGamemodeSettings localGamemodeSettings;

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
            if (Object.HasStateAuthority == false) return;

            for(int i = 0; i < cpus.Count; i++)
            {
                ModObjectReference objectReference = cpus[i].characterReference;
                if(objectReference.IsValid() && cpus[i].objectId.IsValid == false)
                {
                    List<PlayerRef> failedLoadPlayers = await LobbyManager.singleton.clientContentLoaderService.TellClientsToLoad<IFighterDefinition>(objectReference);
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
            }
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
            gamemodeSettings = train.localGamemodeSettings;
        }

        public override void AddGamemodeSettings(int player, LobbySettingsMenu settingsMenu, bool local = false)
        {
            TrainingGamemodeSettings gs = local ? localGamemodeSettings : gamemodeSettings;
            IMapDefinition mapDefinition = ContentManager.singleton.GetContentDefinition<IMapDefinition>(gs.map);
            string mapName = mapDefinition != null ? mapDefinition.Name : "None";
            settingsMenu.AddOption("Map", mapName).onSubmit.AddListener(async () => { await OpenMapSelection(player, local); });
        }

        private async UniTask OpenMapSelection(int player, bool local = false)
        {
            await ContentSelect.singleton.OpenMenu<IMapDefinition>(player,(a, b) =>
            {
                ContentSelect.singleton.CloseMenu(player);
                if (local)
                {
                    localGamemodeSettings.map = b;
                    WhenGamemodeSettingsChanged(true);
                }
                else
                {
                    var temp = gamemodeSettings;
                    temp.map = b;
                    gamemodeSettings = temp;
                    WhenGamemodeSettingsChanged();
                }
            });
        }
        #endregion

        public void ApplyExternalSettings(GameModeBase gmb)
        {
            GamemodeTraining gm = gmb as GamemodeTraining;
            gamemodeSettings = gm.localGamemodeSettings;
        }

        public override async UniTask<bool> VerifyGameModeSettings()
        {
            if (NetworkManager.singleton.FusionLauncher.NetworkRunner.IsRunning == false) return true;
            List<PlayerRef> failedLoadPlayers = await LobbyManager.singleton.clientContentLoaderService.TellClientsToLoad<IMapDefinition>(mapReference);
            if (failedLoadPlayers == null)
            {
                Debug.LogError("Load Map Local Failure");
                return false;
            }

            foreach (var v in failedLoadPlayers)
            {
                Debug.Log($"{v.PlayerId} failed to load {mapReference.ToString()}.");
            }

            if (failedLoadPlayers.Count != 0) return false;

            return true;
        }

        public override bool VerifyReference(ModObjectReference reference)
        {
            if (reference == mapReference) return true;
            return false;
        }

        List<List<GameObject>> spawnPoints = new List<List<GameObject>>();
        List<int> spawnPointsCurr = new List<int>();
        public override async void StartGamemode()
        {
            GamemodeState = GameModeState.INITIALIZING;

            await LobbyManager.singleton.clientMapLoaderService.TellClientsToLoad(mapReference);

            IMapDefinition mapDefinition = ContentManager.singleton.GetContentDefinition<IMapDefinition>(mapReference);

            LobbyManager.singleton.currentLoadedScenes.Clear();
            LobbyManager.singleton.currentLoadedScenes.Add(new CustomSceneRef()
            {
                source = mapReference.modIdentifier.Item1,
                modIdentifier = mapReference.modIdentifier.Item2,
                sceneIndex = mapReference.objectIdentifier
            });

            Runner.SetActiveScene(1);

            SpawnPointHolder[] spawnPointHolders = GameObject.FindObjectsOfType<SpawnPointHolder>();

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

            foreach(PlayerRef playerRef in Runner.ActivePlayers)
            {
                ClientManager cm = Runner.GetPlayerObject(playerRef).GetBehaviour<ClientManager>();

                for(int x = 0; x < cm.ClientPlayers.Count; x++)
                {
                    var clientPlayer = cm.ClientPlayers[x];

                    NetworkObject no = cm.SpawnPlayer(playerRef, x, GetSpawnPosition(cm, clientPlayer));
                    spawnPointsCurr[clientPlayer.team]++;
                }
            }
        }

        private Vector3 GetSpawnPosition(ClientManager cm, ClientPlayerDefinition clientPlayer)
        {
            return spawnPoints[clientPlayer.team][spawnPointsCurr[clientPlayer.team] % spawnPoints[clientPlayer.team].Count].transform.position;
        }

        /*
        public override void FixedUpdateNetwork()
        {
            for(int i = 0; i < cpus.Count; i++)
            {
                if (cpus[i].objectId.IsValid == false) continue;
                NetworkObject no = Runner.FindObject(cpus[i].objectId);
                FighterInputManager fim = no.GetBehaviour<FighterInputManager>();
                fim.FeedInput(Runner.Simulation.Tick, CreateBotInput());
            }
        }

        public bool flickerA = false;
        private NetworkPlayerInputData CreateBotInput()
        {
            NetworkPlayerInputData npi = new NetworkPlayerInputData();

            npi.buttons.Set(PlayerInputType.A, flickerA && Runner.Simulation.Tick%5 == 0 ? true : false);

            return npi;
        }*/
    }
}