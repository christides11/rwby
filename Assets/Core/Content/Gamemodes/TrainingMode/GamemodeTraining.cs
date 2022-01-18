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
        [Networked, Capacity(15)] public string characterReference { get => default; set { } }
        public NetworkId objectId;
    }

    public class GamemodeTraining : GameModeBase
    {
        public event EmptyAction OnCPUListUpdated;

        public ModObjectReference botReference;
        public FighterInputManager botInputManager;

        public ModObjectReference mapReference = new ModObjectReference();

        public TrainingSettingsMenu settingsMenu;

        [Networked(OnChanged = nameof(CpuListUpdated)), Capacity(4)] public NetworkLinkedList<CPUReference> cpus { get; }

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
                ModObjectReference objectReference = new ModObjectReference(cpus[i].characterReference);
                if(objectReference.IsEmpty() == false && cpus[i].objectId.IsValid == false)
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

        public override void AddGamemodeSettings(LobbyMenu lobbyManager)
        {
            GameObject gamemodeOb = GameObject.Instantiate(lobbyManager.gamemodeOptionsContentPrefab, lobbyManager.gamemodeOptionsList, false);
            TextMeshProUGUI[] textMeshes = gamemodeOb.GetComponentsInChildren<TextMeshProUGUI>();
            textMeshes[0].text = mapReference.ToString();
            gamemodeOb.GetComponentInChildren<PlayerPointerEventTrigger>().OnPointerClickEvent.AddListener((d) => { _ = OpenMapSelection(); });
        }

        private async UniTask OpenMapSelection()
        {
            await ContentSelect.singleton.OpenMenu<IMapDefinition>((a, b) => { 
                ContentSelect.singleton.CloseMenu(); 
                mapReference = b; 
                LobbyManager.singleton.CallGamemodeSettingsChanged(); 
            });
        }

        public override async UniTask<bool> VerifyGameModeSettings()
        {
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

        List<List<GameObject>> spawnPoints = new List<List<GameObject>>();
        List<int> spawnPointsCurr = new List<int>();
        public override async void StartGamemode()
        {
            GamemodeState = GameModeState.INITIALIZING;

            await LobbyManager.singleton.clientMapLoaderService.TellClientsToLoad(mapReference);

            IMapDefinition mapDefinition = ContentManager.singleton.GetContentDefinition<IMapDefinition>(mapReference);

            if (NetworkManager.singleton.FusionLauncher.TryGetSceneRef(out SceneRef scene))
            {
                Runner.SetActiveScene(scene);
            }

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

        public override void FixedUpdateNetwork()
        {

        }
    }
}