using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class LobbySettingsManager
    {
        public LobbyManager lobbyManager;

        public LobbySettingsManager(LobbyManager lobbyManager)
        {
            this.lobbyManager = lobbyManager;
        }
        
        public async UniTask<bool> VerifyMatchSettings()
        {
            if (lobbyManager.CurrentGameMode == null) return false;

            IGameModeDefinition gamemodeDefinition =
                ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(lobbyManager.Settings.gamemodeReference);
            if (VerifyTeams(gamemodeDefinition) == false) return false;
            if (await lobbyManager.CurrentGameMode.VerifyGameModeSettings() == false) return false;
            return true;
        }
        
        public async UniTask<bool> TrySetGamemode(ModObjectReference gamemodeReference)
        {
            if (lobbyManager.Object.HasStateAuthority == false) return false;

            List<PlayerRef> failedLoadPlayers =
                await lobbyManager.clientContentLoaderService.TellClientsToLoad<IGameModeDefinition>(gamemodeReference);
            if (failedLoadPlayers == null)
            {
                Debug.LogError("Set Gamemode Local Failure");
                return false;
            }

            foreach (var v in failedLoadPlayers)
            {
                Debug.Log($"{v.PlayerId} failed to load {gamemodeReference.ToString()}.");
            }

            if (lobbyManager.CurrentGameMode != null)
            {
                lobbyManager.Runner.Despawn(lobbyManager.CurrentGameMode.GetComponent<NetworkObject>());
            }

            for (int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                var tempList = ClientManager.clientManagers[i].ClientPlayers;
                for (int k = 0; k < tempList.Count; k++)
                {
                    ClientPlayerDefinition clientPlayerDef = tempList[k];
                    clientPlayerDef.team = 0;
                    tempList[k] = clientPlayerDef;
                }
            }

            IGameModeDefinition gamemodeDefinition =
                ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(gamemodeReference);
            GameObject gamemodePrefab = gamemodeDefinition.GetGamemode();
            lobbyManager.CurrentGameMode = lobbyManager.Runner.Spawn(gamemodePrefab.GetComponent<GameModeBase>(), Vector3.zero, Quaternion.identity);

            LobbySettings temp = lobbyManager.Settings;
            temp.gamemodeReference = gamemodeReference;
            temp.teams = (byte)gamemodeDefinition.maximumTeams;
            lobbyManager.Settings = temp;
            return true;
        }

        public void SetMaxPlayersPerClient(int count)
        {
            var temp = lobbyManager.Settings;
            temp.maxPlayersPerClient = count;
            lobbyManager.Settings = temp;
        }
        
        #region TEAMS

        public void SetTeamCount(byte count)
        {
            var temp = lobbyManager.Settings;
            temp.teams = count;
            lobbyManager.Settings = temp;
        }
        
        private bool VerifyTeams(IGameModeDefinition gamemodeDefiniton)
        {
            int[] teamCount = new int[lobbyManager.Settings.teams];

            for (int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                for (int k = 0; k < ClientManager.clientManagers[i].ClientPlayers.Count; k++)
                {
                    byte playerTeam = ClientManager.clientManagers[i].ClientPlayers[k].team;
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
        
        public TeamDefinition GetTeamDefinition(int team)
        {
            if (lobbyManager.CurrentGameMode == null) return new TeamDefinition();
            if (team < 0 || team > lobbyManager.Settings.teams) return new TeamDefinition();
            if (team == 0)
            {
                return lobbyManager.CurrentGameMode.definition.defaultTeam;
            }
            else
            {
                return lobbyManager.CurrentGameMode.definition.teams[team-1];
            }
        }
        #endregion
    }
}