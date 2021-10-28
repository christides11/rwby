using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;

namespace rwby
{
    public class NetworkManager : MonoBehaviour
    {

        public static NetworkManager singleton;

        public FusionLauncher FusionLauncher { get { return fusionLauncher; } }

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

        public void StartSinglePlayerHost()
        {
            LaunchFusion(GameMode.Single, "", 1);
        }

        public void StartHost(string lobbyName, int playerCount, bool privateLobby)
        {
            LaunchFusion(GameMode.Host, lobbyName, playerCount);
        }

        public void StartDedicatedServer(string lobbyName, int playerCount)
        {
            LaunchFusion(GameMode.Server, lobbyName, 0);
        }

        public void JoinHost(string roomName)
        {
            LaunchFusion(GameMode.Client, roomName, 0);
        }

        public void JoinHost(SessionInfo sessionInfo)
        {
            LaunchFusion(sessionInfo);
        }

        private void LaunchFusion(GameMode gamemode, string roomName, int playerCount)
        {
            if (string.IsNullOrEmpty(roomName))
                roomName = $"{Random.Range(0, 10000)}";
            fusionLauncher.Launch(gamemode, roomName, playerCount, false, clientPrefab, OnConnectionStatusUpdate);
        }

        private void LaunchFusion(SessionInfo sessionInfo)
        {
            fusionLauncher.JoinSession(GameMode.Client, sessionInfo, clientPrefab, OnConnectionStatusUpdate);
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