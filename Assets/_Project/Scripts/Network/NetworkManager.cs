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
        public string SessionName { get; set; } = "";

        [SerializeField] private FusionLauncher fusionLauncher;
        [SerializeField] private NetworkObject clientPrefab;
        [SerializeField] private NetworkObject matchManagerPrefab;

        private FusionLauncher.ConnectionStatus connectionStatus;

        public void Awake()
        {
            singleton = this;
        }

        private void Start()
        {
            fusionLauncher.OnStartHosting += SpawnMatchManager;
        }

        public void StartHost()
        {
            SessionName = $"{Random.Range(0, 10000)}";
            LaunchFusion(GameMode.Host, SessionName);
        }

        public void StartDedicatedServer()
        {
            SessionName = $"{Random.Range(0, 10000)}";
            LaunchFusion(GameMode.Server, SessionName);
        }

        public void StartSinglePlayerHost()
        {
            LaunchFusion(GameMode.Single, "");
        }

        public void JoinHost(string roomName)
        {
            SessionName = roomName;
            Debug.Log($"Joining {SessionName}");
            LaunchFusion(GameMode.Client, roomName);
        }

        private void LaunchFusion(GameMode gamemode, string roomName)
        {
            fusionLauncher.Launch(gamemode, roomName, clientPrefab, OnConnectionStatusUpdate);
        }

        private void OnConnectionStatusUpdate(NetworkRunner arg1, FusionLauncher.ConnectionStatus arg2)
        {
            connectionStatus = arg2;
        }

        private void SpawnMatchManager()
        {
            fusionLauncher.NetworkRunner.Spawn(matchManagerPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}