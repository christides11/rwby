using Fusion;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace rwby
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager singleton;
        
        public static readonly string[] regionCodes = new[] { "us", "kr", "sa", "eu", "jp", "asia" };

        public NetworkObject clientPrefab;
        
        public int SessionIDCounter { get; protected set; } = 0;
        public Dictionary<int, FusionLauncher> sessions = new Dictionary<int, FusionLauncher>();

        public void Awake()
        {
            singleton = this;
        }

        public int CreateOrGetSessionHandler(int sessionHandlerID)
        {
            if (sessions.ContainsKey(sessionHandlerID))
            {
                return sessionHandlerID;
            }
            return CreateSessionHandler();
        }
        
        public int CreateSessionHandler()
        {
            SessionIDCounter++;
            FusionLauncher newHandler = gameObject.AddComponent<FusionLauncher>();
            newHandler.clientPrefab = clientPrefab;
            newHandler.sessionID = SessionIDCounter;
            sessions.Add(SessionIDCounter, newHandler);
            return SessionIDCounter;
        }

        public void DestroySessionHandler(int id)
        {
            if (!sessions.ContainsKey(id)) return;
            sessions[id].LeaveSession();
            Destroy(sessions[id]);
            sessions.Remove(id);
        }

        public async UniTask<(bool, int)> TryJoinSession(string sessionName, string password)
        {
            var sessionHandlerID = CreateSessionHandler();
            var sessionHandler = GetSessionHandler(sessionHandlerID);
            var joinSessionLobbyResult = await sessionHandler.JoinSessionLobby();
            if (!joinSessionLobbyResult.Ok)
            {
                Debug.LogError($"Join Session Lobby Error: {joinSessionLobbyResult.ShutdownReason.ToString()}");
                DestroySessionHandler(sessionHandlerID);
                return (false, -1);
            }
            
            var joinSessionResult = await sessionHandler.JoinSession(sessionName);
            if (!joinSessionLobbyResult.Ok)
            {
                Debug.LogError($"Join Session Error: {joinSessionResult.ShutdownReason.ToString()}");
                DestroySessionHandler(sessionHandlerID);
                return (false, -1);
            }
            
            await UniTask.WaitUntil(() => sessionHandler.sessionManager != null);
            
            return (true, sessionHandlerID);
        }

        public FusionLauncher GetSessionHandler(int id)
        {
            return sessions[id];
        }
        
        public int GetSessionHandlerIDByRunner(NetworkRunner runner)
        {
            foreach (var sessionsKey in sessions.Keys)
            {
                if (sessions[sessionsKey]._runner == runner) return sessionsKey;
            }
            return -1;
        }

        public FusionLauncher GetSessionHandlerByRunner(NetworkRunner runner)
        {
            foreach (var sessionsKey in sessions.Keys)
            {
                if (sessions[sessionsKey]._runner == runner) return sessions[sessionsKey];
            }
            return null;
        }
    }
}