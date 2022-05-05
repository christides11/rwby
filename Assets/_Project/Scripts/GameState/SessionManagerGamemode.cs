using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class SessionManagerGamemode : SessionManagerBase
    {
        public delegate void SessionGamemodeAction(SessionManagerGamemode sessionManager);

        public event SessionGamemodeAction OnLobbySettingsChanged;
        public event SessionGamemodeAction OnCurrentGamemodeChanged;
        public event SessionGamemodeAction OnGamemodeSettingsChanged;
        public event SessionGamemodeAction OnClientDefinitionsChanged;
        
        [Networked(OnChanged = nameof(OnChangedGamemodeSettings))] public SessionGamemodeSettings GamemodeSettings { get; set; }
        [Networked(OnChanged = nameof(OnChangedCurrentGameMode))] public GameModeBase CurrentGameMode { get; set; }
        [Networked(OnChanged = nameof(OnChangedClientDefinitions)), Capacity(8)] public NetworkLinkedList<SessionGamemodeClientContainer> ClientDefinitions => default;
        
        protected static void OnChangedGamemodeSettings(Changed<SessionManagerGamemode> changed)
        {
            changed.Behaviour.OnLobbySettingsChanged?.Invoke(changed.Behaviour);
        }
        
        protected static void OnChangedCurrentGameMode(Changed<SessionManagerGamemode> changed)
        {
            changed.Behaviour.OnCurrentGamemodeChanged?.Invoke(changed.Behaviour);
        }
        
        protected static void OnChangedClientDefinitions(Changed<SessionManagerGamemode> changed)
        {
            changed.Behaviour.OnClientDefinitionsChanged?.Invoke(changed.Behaviour);
        }

        public override void Spawned()
        {
            base.Spawned();
            GamemodeSettings = new SessionGamemodeSettings();
        }
        
        public async UniTask<bool> TryStartMatch()
        {
            if (Runner.IsServer == false)
            {
                Debug.LogError("START MATCH ERROR: Client trying to start match.");
                return false;
            }
            
            if (await VerifyMatchSettings() == false)
            {
                Debug.LogError("START MATCH ERROR: Match settings invalid.");
                return false;
            }
            
            HashSet<ModObjectReference> fightersToLoad = new HashSet<ModObjectReference>();

            for (int i = 0; i < ClientDefinitions.Count; i++)
            {
                for (int j = 0; j < ClientDefinitions[i].players.Count; j++)
                {
                    for (int chara = 0; chara < ClientDefinitions[i].players[j].characterReferences.Count; chara++)
                    {
                        if (!ClientDefinitions[i].players[j].characterReferences[chara].IsValid())
                        {
                            Debug.LogError($" has an invalid character reference.");
                            return false;
                        }   
                        
                        fightersToLoad.Add(ClientDefinitions[i].players[j].characterReferences[chara]);
                    }
                }
            }

            foreach (var fighterStr in fightersToLoad)
            {
                List<PlayerRef> failedLoadPlayers =
                    await clientContentLoaderService.TellClientsToLoad<IFighterDefinition>(fighterStr);
                if (failedLoadPlayers == null || failedLoadPlayers.Count > 0)
                {
                    Debug.LogError($"START MATCH ERROR: Player failed to load fighter.");
                    return false;
                }
            }

            Debug.Log("Starting gamemode.");
            CurrentGameMode.StartGamemode();
            return true;
        }
        
        public async UniTask<bool> VerifyMatchSettings()
        {
            if (CurrentGameMode == null) return false;

            IGameModeDefinition gamemodeDefinition = ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(GamemodeSettings.gamemodeReference);
            if (VerifyTeams(gamemodeDefinition) == false) return false;
            if (await CurrentGameMode.VerifyGameModeSettings() == false) return false;
            return true;
        }
        
        public async UniTask<bool> TrySetGamemode(ModObjectReference gamemodeReference)
        {
            if (Object.HasStateAuthority == false) return false;

            List<PlayerRef> failedLoadPlayers = await clientContentLoaderService.TellClientsToLoad<IGameModeDefinition>(gamemodeReference);
            if (failedLoadPlayers == null)
            {
                Debug.LogError("Set Gamemode Local Failure");
                return false;
            }

            foreach (var v in failedLoadPlayers)
            {
                Debug.LogError($"{v.PlayerId} failed to load {gamemodeReference.ToString()}.");
            }

            if (CurrentGameMode != null)
            {
                Runner.Despawn(CurrentGameMode.GetComponent<NetworkObject>());
            }

            for (int i = 0; i < ClientDefinitions.Count; i++)
            {
                var clientDefinition = ClientDefinitions[i];
                ClientManager cm = Runner.GetPlayerObject(ClientDefinitions[i].clientRef).GetBehaviour<ClientManager>();
                for (int j = 0; j < ClientDefinitions[i].players.Count; j++)
                {
                    var clientPlayerDef = clientDefinition.players;
                    var t = clientPlayerDef[j];
                    t.team = 0;
                    clientPlayerDef[j] = t;
                }
            }

            IGameModeDefinition gamemodeDefinition =
                ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(gamemodeReference);
            GameObject gamemodePrefab = gamemodeDefinition.GetGamemode();
            CurrentGameMode = Runner.Spawn(gamemodePrefab.GetComponent<GameModeBase>(), Vector3.zero, Quaternion.identity, onBeforeSpawned:
                (runner, o) => { o.GetBehaviour<GameModeBase>().sessionManager = this; });

            SessionGamemodeSettings temp = GamemodeSettings;
            temp.gamemodeReference = gamemodeReference; 
            GamemodeSettings = temp;
            
            teams = (byte)gamemodeDefinition.maximumTeams;
            return true;
        }

        public TeamDefinition GetTeamDefinition(int team)
        {
            if (CurrentGameMode == null) return new TeamDefinition();
            if (team < 0 || team > teams) return new TeamDefinition();
            if (team == 0)
            {
                return CurrentGameMode.definition.defaultTeam;
            }
            else
            {
                return CurrentGameMode.definition.teams[team-1];
            }
        }
        
        private bool VerifyTeams(IGameModeDefinition gamemodeDefiniton)
        {
            int[] teamCount = new int[teams];

            for (int i = 0; i < ClientDefinitions.Count; i++)
            {
                for (int j = 0; j < ClientDefinitions[i].players.Count; j++)
                {
                    byte playerTeam = ClientDefinitions[i].players[j].team;
                    if (playerTeam == 0) return false;
                    teamCount[playerTeam - 1]++;
                }
            }

            for (int w = 0; w < teamCount.Length; w++)
            {
                if (teamCount[w] > gamemodeDefiniton.teams[w].maximumPlayers
                    || teamCount[w] < gamemodeDefiniton.teams[w].minimumPlayers) return false;
            }

            return true;
        }

        public override void InitializeClient(ClientManager clientManager)
        {
            ClientDefinitions.Add(new SessionGamemodeClientContainer(){ clientRef = clientManager.Object.InputAuthority });
            UpdateClientPlayerCount(clientManager, 0);
        }
        
        public override void UpdateClientPlayerCount(ClientManager clientManager, uint oldAmount)
        {
            for (int i = 0; i < ClientDefinitions.Count; i++)
            {
                if (ClientDefinitions[i].clientRef != clientManager.Object.InputAuthority) continue;
                var clientDefinitions = ClientDefinitions;
                var temp = clientDefinitions[i];
                var clientPlayers = temp.players;
                
                while (clientPlayers.Count > clientManager.ClientPlayerAmount)
                {
                    clientPlayers.Remove(clientPlayers[^1]);
                }

                while (clientPlayers.Count < clientManager.ClientPlayerAmount)
                {
                    clientPlayers.Add(new SessionGamemodePlayerDefinition(){ team = 0 });
                }

                clientDefinitions.Set(i, temp);
            }
        }

        public SessionGamemodeClientContainer GetClientInformation(PlayerRef client)
        {
            foreach (var c in ClientDefinitions)
            {
                if (c.clientRef == client) return c;
            }
            return default;
        }

        public void CLIENT_SetPlayerCharacterCount(int playerID, int count)
        {
            RPC_SetPlayerCharacterCount(playerID, count);
        }

        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.StateAuthority,
            HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_SetPlayerCharacterCount(int playerID, int characterCount, RpcInfo info = default)
        {
            for (int i = 0; i < ClientDefinitions.Count; i++)
            {
                if (ClientDefinitions[i].clientRef != info.Source) continue;
                var clientDefinitions = ClientDefinitions;
                var temp = clientDefinitions[i];
                var clientPlayers = temp.players;
                var playerTemp = clientPlayers[playerID];
                var playerCharacterRefs = playerTemp.characterReferences;
                
                while (playerCharacterRefs.Count > characterCount)
                {
                    playerCharacterRefs.Remove(playerCharacterRefs.Get(playerCharacterRefs.Count-1));
                }

                while (playerCharacterRefs.Count < characterCount)
                {
                    playerCharacterRefs.Add(new ModObjectReference());
                }

                clientPlayers.Set(playerID, playerTemp);
                clientDefinitions.Set(i, temp);
                return;
            }
            Debug.LogError("Could not find client.");
        }
        
        public void CLIENT_SetPlayerCharacter(int playerID, int characterIndex, ModObjectReference characterReference)
        {
            RPC_SetPlayerCharacter(playerID, characterIndex, characterReference);
        }

        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.StateAuthority,
            HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_SetPlayerCharacter(int playerID, int characterIndex, ModObjectReference characterReference, RpcInfo info = default)
        {
            for (int i = 0; i < ClientDefinitions.Count; i++)
            {
                if (ClientDefinitions[i].clientRef != info.Source) continue;
                var clientDefinitions = ClientDefinitions;
                var temp = clientDefinitions[i];
                var clientPlayers = temp.players;
                var playerTemp = clientPlayers[playerID];
                var playerCharacterRefs = playerTemp.characterReferences;

                playerCharacterRefs.Set(characterIndex, characterReference);

                clientPlayers.Set(playerID, playerTemp);
                clientDefinitions.Set(i, temp);
                Debug.Log(clientDefinitions[i].players[playerID].characterReferences[characterIndex].ToString());
                return;
            }
            Debug.LogError("Could not find client.");
        }
    }
}