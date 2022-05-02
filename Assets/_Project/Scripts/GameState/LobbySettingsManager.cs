using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    // TODO: Link to given session manager.
    [System.Serializable]
    public class LobbySettingsManager
    {
        [FormerlySerializedAs("lobbyManager")] public SessionManagerClassic sessionManagerClassic;

        public LobbySettingsManager(SessionManagerClassic sessionManagerClassic)
        {
            this.sessionManagerClassic = sessionManagerClassic;
        }
        
        public async UniTask<bool> VerifyMatchSettings()
        {
            if (sessionManagerClassic.CurrentGameMode == null) return false;

            IGameModeDefinition gamemodeDefinition =
                ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(sessionManagerClassic.GamemodeSettings.gamemodeReference);
            if (VerifyTeams(gamemodeDefinition) == false) return false;
            if (await sessionManagerClassic.CurrentGameMode.VerifyGameModeSettings() == false) return false;
            return true;
        }
        
        public async UniTask<bool> TrySetGamemode(ModObjectReference gamemodeReference)
        {
            if (sessionManagerClassic.Object.HasStateAuthority == false) return false;

            List<PlayerRef> failedLoadPlayers =
                await sessionManagerClassic.clientContentLoaderService.TellClientsToLoad<IGameModeDefinition>(gamemodeReference);
            if (failedLoadPlayers == null)
            {
                Debug.LogError("Set Gamemode Local Failure");
                return false;
            }

            foreach (var v in failedLoadPlayers)
            {
                Debug.Log($"{v.PlayerId} failed to load {gamemodeReference.ToString()}.");
            }

            if (sessionManagerClassic.CurrentGameMode != null)
            {
                sessionManagerClassic.Runner.Despawn(sessionManagerClassic.CurrentGameMode.GetComponent<NetworkObject>());
            }

            /*
            for (int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                var tempList = ClientManager.clientManagers[i].ClientPlayers;
                for (int k = 0; k < tempList.Count; k++)
                {
                    ClientPlayerDefinition clientPlayerDef = tempList[k];
                    clientPlayerDef.team = 0;
                    tempList[k] = clientPlayerDef;
                }
            }*/

            IGameModeDefinition gamemodeDefinition =
                ContentManager.singleton.GetContentDefinition<IGameModeDefinition>(gamemodeReference);
            GameObject gamemodePrefab = gamemodeDefinition.GetGamemode();
            sessionManagerClassic.CurrentGameMode = sessionManagerClassic.Runner.Spawn(gamemodePrefab.GetComponent<GameModeBase>(), Vector3.zero, Quaternion.identity);

            SessionGamemodeSettings temp = sessionManagerClassic.GamemodeSettings;
            temp.gamemodeReference = gamemodeReference;
            temp.teams = (byte)gamemodeDefinition.maximumTeams;
            sessionManagerClassic.GamemodeSettings = temp;
            return true;
        }

        public void SetMaxPlayersPerClient(int count)
        {
            var temp = sessionManagerClassic.GamemodeSettings;
            temp.maxPlayersPerClient = count;
            sessionManagerClassic.GamemodeSettings = temp;
        }
        
        #region TEAMS

        public void SetTeamCount(byte count)
        {
            var temp = sessionManagerClassic.GamemodeSettings;
            temp.teams = count;
            sessionManagerClassic.GamemodeSettings = temp;
        }
        
        private bool VerifyTeams(IGameModeDefinition gamemodeDefiniton)
        {
            int[] teamCount = new int[sessionManagerClassic.GamemodeSettings.teams];

            /*
            for (int i = 0; i < ClientManager.clientManagers.Count; i++)
            {
                for (int k = 0; k < ClientManager.clientManagers[i].ClientPlayers.Count; k++)
                {
                    byte playerTeam = ClientManager.clientManagers[i].ClientPlayers[k].team;
                    if (playerTeam == 0) return false;
                    teamCount[playerTeam - 1]++;
                }
            }*/

            for (int w = 0; w < teamCount.Length; w++)
            {
                if (teamCount[w] > gamemodeDefiniton.teams[w].maximumPlayers
                    || teamCount[w] < gamemodeDefiniton.teams[w].minimumPlayers) return false;
            }

            return true;
        }
        
        public TeamDefinition GetTeamDefinition(int team)
        {
            if (sessionManagerClassic.CurrentGameMode == null) return new TeamDefinition();
            if (team < 0 || team > sessionManagerClassic.GamemodeSettings.teams) return new TeamDefinition();
            if (team == 0)
            {
                return sessionManagerClassic.CurrentGameMode.definition.defaultTeam;
            }
            else
            {
                return sessionManagerClassic.CurrentGameMode.definition.teams[team-1];
            }
        }
        #endregion
    }
}