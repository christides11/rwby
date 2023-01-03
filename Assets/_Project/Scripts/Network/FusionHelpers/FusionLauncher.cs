using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace rwby
{
	public class FusionLauncher : MonoBehaviour, INetworkRunnerCallbacks
	{
		public enum ConnectionStatus { Disconnected, Connecting, Failed, Connected }

		public delegate void EmptyAction();
		public delegate void SessionAction(NetworkRunner runner, int sessionHandlerID);
		public delegate void SessionListAction(NetworkRunner runner, List<SessionInfo> sessionList);
		public delegate void ConnectionAction(NetworkRunner runner);
		public delegate void ConnectionStatusAction(NetworkRunner runner, ConnectionStatus status);
		public delegate void PlayerAction(NetworkRunner runner, PlayerRef player);
		public delegate void ConnectFailedAction(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason);

		public event EmptyAction OnStartHosting;
		public event EmptyAction OnHostingFailed;
		public event EmptyAction OnDedicatedHostingFailed;
		public event ConnectionStatusAction OnConnectionStatusChanged;
		public event ConnectionAction ClientOnConnectedToServer;
		public event ConnectionAction ClientOnDisconnectedFromServer;
		public event PlayerAction HostOnPlayerJoin;
		public event PlayerAction HostOnPlayerLeave;
		public event ConnectFailedAction OnConnectionFailed;
		public event SessionListAction OnSessionsUpdated;
		public event SessionAction OnSessionShutdown;

		public ConnectionStatus Status { get { return _status; } }
		public Dictionary<PlayerRef, NetworkObject> Players { get { return _players; } }
		public NetworkRunner _runner { get; private set; } = null;

		private Action<NetworkRunner, ConnectionStatus> _connectionCallback;
		private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();
		private ConnectionStatus _status;
		private FusionObjectPoolRoot _pool;
		[FormerlySerializedAs("networkSceneManagerBase")] public CustomNetworkSceneManagerBase netSceneManager;

		private GameMode _gamemode;
		
		[FormerlySerializedAs("_playerPrefab")] public NetworkObject clientPrefab;

		public int sessionID;
		
		public SessionManagerBase sessionManager;

		public List<CustomSceneRef> defaultSceneList = new List<CustomSceneRef>()
		{
			new CustomSceneRef(new ContentGUID(8), 0, 0),
			new CustomSceneRef(new ContentGUID(8), 0, 1)
		};

		private string password;
		
		public List<CustomSceneRef> GetCurrentScenes()
		{
			if (!sessionManager)
			{
				return defaultSceneList;
			}

			return sessionManager.currentLoadedScenes.ToList();
		}

		private void OnConnectionStatusUpdate(NetworkRunner arg1, FusionLauncher.ConnectionStatus status)
		{
			_status = status;
			OnConnectionStatusChanged?.Invoke(_runner, status);
		}

		public async UniTask<StartGameResult> JoinSessionLobby()
		{
			if (!_runner)
				_runner = gameObject.AddComponent<NetworkRunner>();
			_runner.AddCallbacks(this);
			
			var result = await _runner.JoinSessionLobby(SessionLobby.ClientServer);
			return result;
		}

		public async UniTask<StartGameResult> DedicateHostSession(string roomName, int playerCount, string password)
		{
			//clientPrefab = playerPrefab;
			_connectionCallback = OnConnectionStatusUpdate;
			InitSingletions(false);
			
			var customProps = new Dictionary<string, SessionProperty>();
			customProps["name"] = roomName;
			customProps["map"] = "";
			customProps["gamemode"] = "";
			customProps["modhash"] = "";
			customProps["password"] = String.IsNullOrEmpty(password) ? 0 : 1;

			this.password = this.password;

			_gamemode = GameMode.Server;
			StartGameResult result = await _runner.StartGame(new StartGameArgs()
			{
				GameMode = GameMode.Server, 
				SessionProperties = customProps,
				ObjectPool = _pool, 
				SceneManager = netSceneManager, 
				PlayerCount = playerCount,
			});
			if(result.Ok == false)
            {
				Debug.LogError(result.ShutdownReason);
				OnDedicatedHostingFailed?.Invoke();
				return result;
            }
			return result;
		}

		public async UniTask<StartGameResult> HostSession(string roomName, int playerCount, string password, bool local = false)
		{
			_connectionCallback = OnConnectionStatusUpdate;
			InitSingletions(true);

			var customProps = new Dictionary<string, SessionProperty>();
			customProps["name"] = roomName;
			customProps["map"] = "";
			customProps["gamemode"] = "";
			customProps["modhash"] = "";
			customProps["password"] = String.IsNullOrEmpty(password) ? 0 : 1;

			this.password = password;
			
			_gamemode = GameMode.Host;
			StartGameResult result = await _runner.StartGame(new StartGameArgs()
			{
				GameMode = local ? GameMode.Single : GameMode.Host,
				SessionProperties = customProps,
				ObjectPool = _pool,
				SceneManager = netSceneManager,
				PlayerCount = playerCount
			});
			if (result.Ok == false)
			{
				Debug.LogError(result.ShutdownReason);
				OnHostingFailed?.Invoke();
			}
			return result;
		}

		public async UniTask<StartGameResult> JoinSession(SessionInfo session, string password = "")
		{
			return await JoinSession(session.Name, password);
		}

		public async UniTask<StartGameResult> JoinSession(string sessionName, string password = "")
		{
			_connectionCallback = OnConnectionStatusUpdate;

			InitSingletions(true);
			_gamemode = GameMode.Client;
			var result = await _runner.StartGame(new StartGameArgs()
			{
				GameMode = GameMode.Client, 
				SessionName = sessionName, 
				ObjectPool = _pool,
				SceneManager = netSceneManager,
				DisableClientSessionCreation = true,
				ConnectionToken = string.IsNullOrEmpty(password) ? null : Encoding.ASCII.GetBytes(password)
			});
			return result;
		}

		public void LeaveSession()
		{
			if (_runner != null) _runner.Shutdown();
		}

		protected void InitSingletions(bool provideInput)
		{
			if (!_runner)
				_runner = gameObject.AddComponent<NetworkRunner>();
			_runner.name = name;
			_runner.ProvideInput = provideInput;

			if (!_pool)
				_pool = gameObject.AddComponent<FusionObjectPoolRoot>();
			if (!netSceneManager)
				netSceneManager = gameObject.AddComponent<CustomNetworkSceneManager>();
		}

		public void OnInput(NetworkRunner runner, NetworkInput input){}
		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input){}
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
		{
			Debug.Log("Client requested connection.");
			if (string.IsNullOrEmpty(password))
			{
				request.Accept();
				return;
			}

			if (token == null || token.Length == 0)
			{
				request.Refuse();
				return;
			}
			string someString = Encoding.ASCII.GetString(token);
			if (someString != password)
			{
				request.Refuse();
				return;
			}
			request.Accept();
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
			_players[player] = runner.Spawn(clientPrefab, Vector3.zero, Quaternion.identity, player);
			runner.SetPlayerObject(player, _players[player]);
			if (runner.IsServer)
			{
				if (_gamemode == GameMode.Host)
				{
					OnStartHosting?.Invoke();
				}
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
			OnSessionShutdown?.Invoke(runner, sessionID);
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