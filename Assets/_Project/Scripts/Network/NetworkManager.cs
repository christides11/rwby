using Cysharp.Threading.Tasks;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class NetworkManager : MonoBehaviour
    {

        public static NetworkManager singleton;

        public FusionLauncher FusionLauncher { get { return fusionLauncher; } }

        [SerializeField] private FusionLauncher fusionLauncher;
        [SerializeField] private NetworkObject clientPrefab;
        [SerializeField] private NetworkObject lobbyManagerPrefab;

        public void Awake()
        {
            singleton = this;
        }

        private void Start()
        {
            fusionLauncher.OnStartHosting += SpawnMatchManager;
        }

        public void StartSinglePlayerHost()
        {
            _ = fusionLauncher.HostSession("localSession", 8, false, clientPrefab, true);
        }

        public void StartHost(string lobbyName, int playerCount, bool privateLobby)
        {
            if (string.IsNullOrEmpty(lobbyName))
                lobbyName = $"{Random.Range(0, 10000)}";
            _ = fusionLauncher.HostSession(lobbyName, playerCount, false, clientPrefab);
        }

        public void StartDedicatedServer(string lobbyName, int playerCount)
        {
            if (string.IsNullOrEmpty(lobbyName))
                lobbyName = $"{Random.Range(0, 10000)}";
            _ = fusionLauncher.DedicateHostSession(lobbyName, playerCount, false, clientPrefab);
        }

        public void JoinHost(string roomName)
        {
            _ = fusionLauncher.JoinSession(roomName, clientPrefab);
        }

        public void JoinHost(SessionInfo sessionInfo)
        {
            _ = fusionLauncher.JoinSession(sessionInfo, clientPrefab);
        }

        private void SpawnMatchManager()
        {
            fusionLauncher.NetworkRunner.Spawn(lobbyManagerPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}