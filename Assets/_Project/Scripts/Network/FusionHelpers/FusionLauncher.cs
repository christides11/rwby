using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rwby
{
	public class FusionLauncher : MonoBehaviour, INetworkRunnerCallbacks
	{
		public enum ConnectionStatus { Disconnected, Connecting, Failed, Connected }

		public delegate void EmptyAction();
		public delegate void SessionListAction(NetworkRunner runner, List<SessionInfo> sessionList);
		public delegate void ConnectionAction(NetworkRunner runner);
		public delegate void ConnectionStatusAction(NetworkRunner runner, ConnectionStatus status);
		public delegate void PlayerAction(NetworkRunner runner, PlayerRef player);
		public delegate void ConnectFailedAction(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason);

		public event EmptyAction OnStartHosting;
		public event EmptyAction OnHostingFailed;
		public event ConnectionStatusAction OnConnectionStatusChanged;
		public event ConnectionAction ClientOnConnectedToServer;
		public event ConnectionAction ClientOnDisconnectedFromServer;
		public event PlayerAction HostOnPlayerJoin;
		public event PlayerAction HostOnPlayerLeave;
		public event ConnectFailedAction OnConnectionFailed;
		public event SessionListAction OnSessionsUpdated;

		public ConnectionStatus Status { get { return _status; } }
		public Dictionary<PlayerRef, NetworkObject> Players { get { return _players; } }
		public NetworkRunner NetworkRunner { get { return _runner; } }

		private NetworkRunner _runner;
		private Action<NetworkRunner, ConnectionStatus> _connectionCallback;
		private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();
		private ConnectionStatus _status;
		private NetworkObject _playerPrefab;
		private FusionObjectPoolRoot _pool;
		public NetworkSceneManager _networkSceneManager;

		private void OnConnectionStatusUpdate(NetworkRunner arg1, FusionLauncher.ConnectionStatus status)
		{
			_status = status;
			OnConnectionStatusChanged?.Invoke(_runner, status);
		}

		public bool TryGetSceneRef(out SceneRef sceneRef)
		{
			// Find the current scene in the list of scenes registered with Fusion and return the ref
			var scenePath = SceneManager.GetActiveScene().path;
			return NetworkSceneManager.TryGetSceneRefFromPathInBuildSettings(scenePath, out sceneRef);
		}

		public async UniTask JoinSessionLobby()
		{
			_runner = gameObject.GetComponent<NetworkRunner>();
			if (!_runner)
				_runner = gameObject.AddComponent<NetworkRunner>();
			_runner.AddCallbacks(this);

			await _runner.JoinSessionLobby(SessionLobby.ClientServer);
		}

		public async UniTask DedicateHostSession(string roomName, int playerCount, bool privateLobby, NetworkObject playerPrefab)
		{
			_playerPrefab = playerPrefab;
			_connectionCallback = OnConnectionStatusUpdate;
			InitSingletions(false);

			await _runner.StartGame(new StartGameArgs() { GameMode = GameMode.Server, SessionName = roomName, ObjectPool = _pool, SceneObjectProvider = _networkSceneManager, PlayerCount = playerCount });
			if (_status == ConnectionStatus.Failed)
			{
				OnHostingFailed?.Invoke();
				return;
			}

			if (TryGetSceneRef(out SceneRef scene))
			{
				_runner.SetActiveScene(scene);
			}
		}

		public async UniTask HostSession(string roomName, int playerCount, bool privateLobby, NetworkObject playerPrefab, bool local = false)
		{
			_playerPrefab = playerPrefab;
			_connectionCallback = OnConnectionStatusUpdate;
			InitSingletions(true);

			await _runner.StartGame(new StartGameArgs()
			{
				GameMode = local ? GameMode.Single : GameMode.Host,
				SessionName = roomName,
				ObjectPool = _pool,
				SceneObjectProvider = _networkSceneManager,
				PlayerCount = playerCount
			});
			if (_status == ConnectionStatus.Failed)
			{
				OnHostingFailed?.Invoke();
				return;
			}

			if (TryGetSceneRef(out SceneRef scene))
			{
				_runner.SetActiveScene(scene);
			}
		}

		public async UniTask JoinSession(SessionInfo session, NetworkObject playerPrefab)
		{
			_playerPrefab = playerPrefab;
			_connectionCallback = OnConnectionStatusUpdate;

			InitSingletions(true);
			//await _runner.StartGame(session);
			if (_status == ConnectionStatus.Failed)
			{
				return;
			}
		}

		public async UniTask JoinSession(string sessionName, NetworkObject playerPrefab)
		{
			_playerPrefab = playerPrefab;
			_connectionCallback = OnConnectionStatusUpdate;

			InitSingletions(true);
			await _runner.StartGame(new StartGameArgs() { GameMode = GameMode.Client, SessionName = sessionName });
			if (_status == ConnectionStatus.Failed)
			{
				return;
			}
		}

		protected void InitSingletions(bool provideInput)
		{
			_runner = gameObject.GetComponent<NetworkRunner>();
			if (!_runner)
				_runner = gameObject.AddComponent<NetworkRunner>();
			_runner.name = name;
			_runner.ProvideInput = provideInput;
			//_runner.AddCallbacks(this);

			if (_pool == null)
				_pool = gameObject.AddComponent<FusionObjectPoolRoot>();

			_networkSceneManager = gameObject.GetComponent<NetworkSceneManager>();
			if (!_networkSceneManager)
				_networkSceneManager = gameObject.AddComponent<NetworkSceneManager>();
		}

		public void OnInput(NetworkRunner runner, NetworkInput input)
		{

		}

		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
		{

		}

		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
		{
			Debug.Log("Client requested connection.");
		}

		public void OnConnectedToServer(NetworkRunner runner)
		{
			Debug.Log("Connected to server");
			ClientOnConnectedToServer?.Invoke(runner);
		}

		public void OnDisconnectedFromServer(NetworkRunner runner)
		{
			Debug.Log("Disconnected from server");
			ClientOnDisconnectedFromServer?.Invoke(runner);
		}

		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
		{
			Debug.Log("Failed to connect to server");
			OnConnectionFailed?.Invoke(runner, remoteAddress, reason);
		}

		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
		{
			Debug.Log($"Player {player.PlayerId} joined the session.");
			_players[player] = runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);
			if (runner.Mode == SimulationModes.Host && runner.LocalPlayer.IsValid && runner.LocalPlayer == player)
			{
				Debug.Log($"Hosting successful.");
				OnStartHosting?.Invoke();
			}
			HostOnPlayerJoin?.Invoke(runner, player);
		}

		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			Debug.Log($"Player {player.PlayerId} left the session.");
			NetworkObject p;
			if (_players.TryGetValue(player, out p))
			{
				runner.Despawn(p);
				_players.Remove(player);
			}
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

		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
		{
			Debug.Log($"OnSessionListUpdated: {sessionList?.Count}");
			OnSessionsUpdated?.Invoke(runner, sessionList);
		}

		public void OnSceneLoadDone(NetworkRunner runner)
		{

		}

		public void OnSceneLoadStart(NetworkRunner runner)
		{

		}

		public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
		{

		}

		public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
		{

		}
	}
}