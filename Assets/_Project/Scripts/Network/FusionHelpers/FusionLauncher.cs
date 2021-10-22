using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rwby
{
	/// <summary>
	/// Small helper that reduces the Fusion setup and connection work to a single line of code.
	/// To host a new session, call
	/// * FusionLauncher.Launch(mode,room,playerPrefab,callback)
	/// </summary>

	public class FusionLauncher : MonoBehaviour, INetworkRunnerCallbacks
	{
		public delegate void EmptyAction();
		public delegate void ConnectionAction(NetworkRunner runner);
		public delegate void PlayerAction(NetworkRunner runner, PlayerRef player);
		public delegate void ConnectFailedAction(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason);

		public event EmptyAction OnStartHosting;
		public event ConnectionAction ClientOnConnectedToServer;
		public event ConnectionAction ClientOnDisconnectedFromServer;
		public event PlayerAction HostOnPlayerJoin;
		public event PlayerAction HostOnPlayerLeave;
		public event ConnectFailedAction OnConnectionFailed;

		public ConnectionStatus Status { get { return _status; } }
		public Dictionary<PlayerRef, NetworkObject> Players { get { return _players; } }
		public NetworkRunner NetworkRunner { get { return _runner; } }

		private NetworkRunner _runner;
		private Action<NetworkRunner, ConnectionStatus> _connectionCallback;
		private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();
		private ConnectionStatus _status;
		private NetworkObject _playerPrefab;
		private FusionObjectPoolRoot _pool;

		public enum ConnectionStatus { Disconnected, Connecting, Failed, Connected }

		public async void Launch(GameMode mode, string roomName, NetworkObject playerPrefab, Action<NetworkRunner, ConnectionStatus> onConnect)
		{
			_playerPrefab = playerPrefab;
			_connectionCallback = onConnect;

			SetConnectionStatus(ConnectionStatus.Connecting);

			//NetworkProjectConfigAsset.Instance.NetworkObjectPool = ScriptableObject.CreateInstance<FusionObjectPoolRoot>(); //gameObject.AddComponent<FusionObjectPoolRoot>();
			/*
			if (mode == GameMode.Shared)
				NetworkProjectConfig.Global.Simulation.ReplicationMode = SimulationConfig.StateReplicationModes.EventualConsistency;
			else
				NetworkProjectConfig.Global.Simulation.ReplicationMode = SimulationConfig.StateReplicationModes.DeltaSnapshots;*/

			_runner = gameObject.GetComponent<NetworkRunner>();
			if (!_runner)
				_runner = gameObject.AddComponent<NetworkRunner>();
			_runner.name = name;
			_runner.ProvideInput = mode != GameMode.Server;
			_runner.AddCallbacks(this);

			if (_pool == null)
				_pool = gameObject.AddComponent<FusionObjectPoolRoot>();

			await _runner.StartGame(new StartGameArgs() { GameMode = mode, SessionName = roomName, ObjectPool = _pool });

			var current = SceneManager.GetActiveScene();
			if (mode != GameMode.Client && TryGetSceneRef(out SceneRef scene))
			{
				_runner.SetActiveScene(scene);
			}

			if(mode == GameMode.Host)
            {
				OnStartHosting?.Invoke();
            }
		}

		private bool TryGetSceneRef(out SceneRef sceneRef)
		{
			var scenePath = SceneManager.GetActiveScene().path;
			var config = NetworkProjectConfig.Global;

			if (config.TryGetSceneRef(scenePath, out sceneRef) == false)
			{

				// Failed to find scene by full path, try with just name
				if (config.TryGetSceneRef(SceneManager.GetActiveScene().name, out sceneRef) == false)
				{
					Debug.LogError($"Could not find scene reference to scene {scenePath}, make sure it's added to {nameof(NetworkProjectConfig)}.");
					return false;
				}
			}
			return true;
		}

		private void SetConnectionStatus(ConnectionStatus status)
		{
			_status = status;
			if (_connectionCallback != null)
				_connectionCallback(_runner, status);
		}

		public void OnInput(NetworkRunner runner, NetworkInput input)
		{
		}

		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
		{
		}

		public void OnConnectedToServer(NetworkRunner runner)
		{
			Debug.Log("Connected to server");
			if (runner.GameMode == GameMode.Shared)
				_players[runner.LocalPlayer] = runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
			SetConnectionStatus(ConnectionStatus.Connected);
			ClientOnConnectedToServer?.Invoke(runner);
		}

		public void OnDisconnectedFromServer(NetworkRunner runner)
		{
			Debug.Log("Disconnected from server");
			SetConnectionStatus(ConnectionStatus.Disconnected);
			ClientOnDisconnectedFromServer?.Invoke(runner);
		}

		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
		{
			Debug.Log("Connected request");
		}

		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
		{
			Debug.Log("Connected failed");
			SetConnectionStatus(ConnectionStatus.Failed);
			OnConnectionFailed?.Invoke(runner, remoteAddress, reason);
		}

		// Called on host when new player joins
		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
		{
			Debug.Log("Player Joined");
			_players[player] = runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);
			SetConnectionStatus(ConnectionStatus.Connected);
			HostOnPlayerJoin?.Invoke(runner, player);
		}

		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			Debug.Log("Player Left");
			NetworkObject p;
			if (_players.TryGetValue(player, out p))
			{
				runner.Despawn(p);
				_players.Remove(player);
			}
			SetConnectionStatus(_status);
			HostOnPlayerLeave?.Invoke(runner, player);
		}

		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
		{
		}

		public void OnObjectWordsChanged(NetworkRunner runner, NetworkObject obj, HashSet<int> changedWords, NetworkObjectMemoryPtr oldMemory)
		{
		}

		public void OnShutdown(NetworkRunner runner)
		{
			Debug.Log("Shutdown");
		}

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
			Debug.Log($"Shutdown ({shutdownReason.ToString()})");
		}

		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnSceneLoadDone(NetworkRunner runner)
        {

        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {

        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {

        }
    }
}