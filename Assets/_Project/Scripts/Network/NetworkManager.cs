using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager singleton;

        public NetworkObject clientPrefab;
        
        public int SessionIDCounter { get; protected set; } = 0;
        public Dictionary<int, FusionLauncher> sessions = new Dictionary<int, FusionLauncher>();

        public void Awake()
        {
            singleton = this;
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