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
		public CustomNetworkSceneManagerBase networkSceneManagerBase;

		private void OnConnectionStatusUpdate(NetworkRunner arg1, FusionLauncher.ConnectionStatus status)
		{
			_status = status;
			OnConnectionStatusChanged?.Invoke(_runner, status);
		}

		public async UniTask JoinSessionLobby()
		{
			_runner = gameObject.GetComponent<NetworkRunner>();
			if (!_runner)
				_runner = gameObject.AddComponent<NetworkRunner>();
			_runner.AddCallbacks(this);
			
			await _runner.JoinSessionLobby(SessionLobby.ClientServer);
		}

		public async UniTask<StartGameResult> DedicateHostSession(string roomName, int playerCount, bool privateLobby, NetworkObject playerPrefab)
		{
			_playerPrefab = playerPrefab;
			_connectionCallback = OnConnectionStatusUpdate;
			InitSingletions(false);
			
			var customProps = new Dictionary<string, SessionProperty>();
			customProps["name"] = roomName;
			customProps["map"] = "";
			customProps["gamemode"] = "";

			StartGameResult result = await _runner.StartGame(new StartGameArgs()
			{
				GameMode = GameMode.Server, 
				SessionProperties = customProps,
				SessionName = roomName, 
				ObjectPool = _pool, 
				SceneObjectProvider = networkSceneManagerBase, 
				PlayerCount = playerCount
			});
			if(result.Ok == false)
            {
				Debug.LogError(result.ShutdownReason);
				OnHostingFailed?.Invoke();
				return result;
            }

			return result;
		}

		public async UniTask<StartGameResult> HostSession(string roomName, int playerCount, bool privateLobby, NetworkObject playerPrefab, bool local = false)
		{
			_playerPrefab = playerPrefab;
			_connectionCallback = OnConnectionStatusUpdate;
			InitSingletions(true);

			var customProps = new Dictionary<string, SessionProperty>();
			customProps["name"] = roomName;
			customProps["map"] = "";
			customProps["gamemode"] = "";
			customProps["modhash"] = "";
			
			StartGameResult result = await _runner.StartGame(new StartGameArgs()
			{
				GameMode = local ? GameMode.Single : GameMode.Host,
				SessionProperties = customProps,
				ObjectPool = _pool,
				SceneObjectProvider = networkSceneManagerBase,
				PlayerCount = playerCount
			});
			if (result.Ok == false)
			{
				Debug.LogError(result.ShutdownReason);
				OnHostingFailed?.Invoke();
			}
			return result;
		}

		public async UniTask JoinSession(SessionInfo session, NetworkObject playerPrefab)
		{
			await JoinSession(session.Name, playerPrefab);
		}

		public async UniTask JoinSession(string sessionName, NetworkObject playerPrefab)
		{
			_playerPrefab = playerPrefab;
			_connectionCallback = OnConnectionStatusUpdate;

			InitSingletions(true);
			await _runner.StartGame(new StartGameArgs()
			{
				GameMode = GameMode.Client, 
				SessionName = sessionName, 
				ObjectPool = _pool,
				SceneObjectProvider = networkSceneManagerBase,
				DisableClientSessionCreation = true
			});
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
		}

		public void OnInput(NetworkRunner runner, NetworkInput input){}
		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input){}
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
			_players[player] = runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);
			runner.SetPlayerObject(player, _players[player]);
			if (runner.Mode == SimulationModes.Host && runner.LocalPlayer.IsValid && runner.LocalPlayer == player)
			{
				Debug.Log($"Hosting successful.");
				OnStartHosting?.Invoke();
			}
			else
			{
				Debug.Log($"Player {player.PlayerId} joined the session.");
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

		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
		
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

		public void OnSceneLoadDone(NetworkRunner runner){}

		public void OnSceneLoadStart(NetworkRunner runner){}

		public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data){}

		public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

		public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data){}
	}
}