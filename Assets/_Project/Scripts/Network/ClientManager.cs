using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using rwby.ui;
using UnityEngine;
using Random = System.Random;

namespace rwby
{
	[OrderBefore(typeof(FighterInputManager), typeof(FighterManager))]
	public class ClientManager : NetworkBehaviour, INetworkRunnerCallbacks, IBeforeUpdate, IAfterUpdate, IInputProvider
	{
		public delegate void ClientAction(ClientManager clientManager);
		public static event ClientAction OnPlayerCountChanged;
		
		[Networked(OnChanged = nameof(OnClientPlayerCountChanged))] public uint ClientPlayerAmount { get; set; }

		private NetworkManager networkManager;
		private GameManager gameManager;
		private LocalPlayerManager localPlayerManager;

		private bool sessionHandlerSet = false;
		private FusionLauncher sessionHandler;

		[Networked] public string nickname { get; set; } = "User";

		// INPUT //
		[Networked] public int latestConfirmedInput { get; set; } = 0;
		[Networked, Capacity(10)] public NetworkArray<NetworkClientInputData> inputBuffer { get; }
		[Networked] public int inputBufferPosition { get; set; } = 0;
		[Networked] public int setInputDelay { get; set; } = 0;

		[Networked] public byte mapLoadPercent { get; set; } = 100;
		[Networked] public NetworkBool ReadyStatus { get; set; } = false;

		public List<string> profiles = new List<string>(4);

		public int tempInputDelaySetter = 3;
		
		protected virtual void Awake()
		{
			gameManager = GameManager.singleton;
			localPlayerManager = gameManager.localPlayerManager;
			networkManager = NetworkManager.singleton;
		}
		
		private static void OnClientPlayerCountChanged(Changed<ClientManager> changed)
		{
			OnPlayerCountChanged?.Invoke(changed.Behaviour);
		}
		
		public override void Spawned()
		{
			if (Object.HasInputAuthority)
			{
				Runner.AddCallbacks(this);
				GameManager.singleton.localPlayerManager.OnPlayerCountChanged += WhenPlayerCountChanged;
			}

			if (Object.HasStateAuthority)
			{
				nickname = $"User {UnityEngine.Random.Range(0, 10000)}";
			}
			setInputDelay = tempInputDelaySetter;
			DontDestroyOnLoad(gameObject);
		}

		private void WhenPlayerCountChanged(LocalPlayerManager localplayermanager, int previousplayercount, int currentplaycount)
		{
			RPC_SetPlayerCount((uint)currentplaycount);
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
		}

		public override void Render()
		{
			if (!sessionHandlerSet)
			{
				sessionHandler = GameManager.singleton.networkManager.GetSessionHandlerByRunner(Runner);
				if (!sessionHandler.sessionManager) return;
				if(Runner.IsServer) sessionHandler.sessionManager.InitializeClient(this);
				sessionHandlerSet = true;
			}
		}

		public void CLIENT_SetReadyStatus(bool readyStatus)
		{
			RPC_SetReadyStatus(readyStatus);
		}

		[Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.StateAuthority,
			HostMode = RpcHostMode.SourceIsHostPlayer)]
		private void RPC_SetReadyStatus(NetworkBool status)
		{
			ReadyStatus = status;
		}

		public void CLIENT_SetMapLoadPercentage(byte loadPercentage)
		{
			RPC_SetMapLoadPercentage(loadPercentage);
		}

		[Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.StateAuthority,
			HostMode = RpcHostMode.SourceIsHostPlayer)]
		private void RPC_SetMapLoadPercentage(byte loadPercentage)
		{
			mapLoadPercent = loadPercentage;
		}

		public void CLIENT_SetPlayerCount(uint playerCount)
		{
			RPC_SetPlayerCount(playerCount);
		}

		[Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
		private void RPC_SetPlayerCount(uint playerCount)
		{
			SessionManagerBase sessionManager = GameManager.singleton.networkManager.GetSessionHandlerByRunner(Runner).sessionManager;
			if (playerCount > sessionManager.maxPlayersPerClient || playerCount == ClientPlayerAmount) return;
			uint temp = ClientPlayerAmount;
			ClientPlayerAmount = playerCount;
			sessionManager.UpdateClientPlayerCount(this, temp);
		}

		Vector2[] buttonMovement = new Vector2[8];
		Vector2[] buttonCamera = new Vector2[8];
		Vector3[] buttonCameraForward = new Vector3[8];
		Vector3[] buttonCameraRight = new Vector3[8];
		Vector3[] buttonCameraPos = new Vector3[8];
		bool[] buttonJump = new bool[4];
		bool[] buttonA = new bool[4];
		bool[] buttonB = new bool[4];
		bool[] buttonC = new bool[4];
		bool[] buttonBlock = new bool[4];
		bool[] buttonDash = new bool[4];
		bool[] buttonLockOn = new bool[4];
		bool[] buttonAbility1 = new bool[4];
		bool[] buttonAbility2 = new bool[4];
		bool[] buttonAbility3 = new bool[4];
		bool[] buttonAbility4 = new bool[4];
		bool[] buttonExtra1 = new bool[4];
		bool[] buttonExtra2 = new bool[4];
		bool[] buttonExtra3 = new bool[4];
		bool[] buttonExtra4 = new bool[4];
		public void BeforeUpdate()
		{
			if (Object.HasInputAuthority == false) return;

			for (int j = 0; j < ClientPlayerAmount; j++)
			{
				var player = localPlayerManager.GetPlayer(j);
				if (!player.isValid) continue;
				
				if (player.rewiredPlayer.GetButtonDown(Action.Pause))
                {
                    if (PauseMenu.singleton.IsPlayerPaused(j))
                    {
						PauseMenu.singleton.CloseMenu(j);
                    }
                    else
                    {
	                    PauseMenu.singleton.OpenMenu(j);
                    }
                }

				if (PauseMenu.singleton.IsPlayerPaused(j)) return;

				buttonMovement[j] = player.rewiredPlayer.GetAxis2D(Action.Movement_X, Action.Movement_Y);
				buttonCamera[j] = player.rewiredPlayer.GetAxis2D(Action.Camera_X, Action.Camera_Y);
				buttonCameraForward[j] = player.camera ? player.camera.transform.forward : Vector3.forward;
				buttonCameraRight[j] = player.camera ? player.camera.transform.right : Vector3.right;
				buttonCameraPos[j] = player.camera ? player.camera.transform.position : Vector3.up;
				if (player.rewiredPlayer.GetButton(Action.Block)) buttonBlock[j] = true;
				if (player.rewiredPlayer.GetButton(Action.Dash)) buttonDash[j] = true;
				if (player.rewiredPlayer.GetButton(Action.Lock_On)) buttonLockOn[j] = true;
				if (player.rewiredPlayer.GetButton(Action.Ability_1)) buttonAbility1[j] = true;
				if (player.rewiredPlayer.GetButton(Action.Ability_2)) buttonAbility2[j] = true;
				if (player.rewiredPlayer.GetButton(Action.Ability_3)) buttonAbility3[j] = true;
				if (player.rewiredPlayer.GetButton(Action.Ability_4)) buttonAbility4[j] = true;
				if (player.rewiredPlayer.GetButton(Action.Extra1)) buttonExtra1[j] = true;
				if (player.rewiredPlayer.GetButton(Action.Extra2)) buttonExtra2[j] = true;
				if (player.rewiredPlayer.GetButton(Action.Extra3)) buttonExtra3[j] = true;
				if (player.rewiredPlayer.GetButton(Action.Extra4)) buttonExtra4[j] = true;

				if (player.rewiredPlayer.GetButton(Action.Ability_Menu))
				{
					if (player.rewiredPlayer.GetButton(Action.Jump)) buttonAbility1[j] = true;
					if (player.rewiredPlayer.GetButton(Action.A)) buttonAbility2[j] = true;
					if (player.rewiredPlayer.GetButton(Action.B)) buttonAbility3[j] = true;
					if (player.rewiredPlayer.GetButton(Action.C)) buttonAbility4[j] = true;
				}
				else
				{
					if (player.rewiredPlayer.GetButton(Action.Jump)) buttonJump[j] = true;
					if (player.rewiredPlayer.GetButton(Action.A)) buttonA[j] = true;
					if (player.rewiredPlayer.GetButton(Action.B)) buttonB[j] = true;
					if (player.rewiredPlayer.GetButton(Action.C)) buttonC[j] = true;
				}
			}
		}

		public void AfterUpdate()
		{
			ClearInput(ref buttonJump);
			ClearInput(ref buttonA);
			ClearInput(ref buttonB);
			ClearInput(ref buttonC);
			ClearInput(ref buttonBlock);
			ClearInput(ref buttonDash);
			ClearInput(ref buttonLockOn);
			ClearInput(ref buttonAbility1);
			ClearInput(ref buttonAbility2);
			ClearInput(ref buttonAbility3);
			ClearInput(ref buttonAbility4);
			ClearInput(ref buttonExtra1);
			ClearInput(ref buttonExtra2);
			ClearInput(ref buttonExtra3);
			ClearInput(ref buttonExtra4);
		}

		private void ClearInput(ref bool[] inputList)
		{
			for (int i = 0; i < inputList.Length; i++)
			{
				inputList[i] = false;
			}
		}

		/// <summary>
		/// Get Unity input and store them in a struct for Fusion
		/// </summary>
		/// <param name="runner">The current NetworkRunner</param>
		/// <param name="input">The target input handler that we'll pass our data to</param>
		public void OnInput(NetworkRunner runner, NetworkInput input)
		{
			var frameworkInput = new NetworkClientInputData();

			if (ClientPlayerAmount == 0)
			{
				input.Set(frameworkInput);
				return;
			}

			for(int i = 0; i < ClientPlayerAmount; i++)
            {
				NetworkPlayerInputData playerInput = new NetworkPlayerInputData();

				playerInput.movement = buttonMovement[i];
				playerInput.forward = buttonCameraForward[i];
				playerInput.right = buttonCameraRight[i];
				playerInput.camPos = buttonCameraPos[i];
				playerInput.buttons.Set(PlayerInputType.JUMP, buttonJump[i]);
				playerInput.buttons.Set(PlayerInputType.A, buttonA[i]);
				playerInput.buttons.Set(PlayerInputType.B, buttonB[i]);
				playerInput.buttons.Set(PlayerInputType.C, buttonC[i]);
				playerInput.buttons.Set(PlayerInputType.BLOCK, buttonBlock[i]);
				playerInput.buttons.Set(PlayerInputType.DASH, buttonDash[i]);
				playerInput.buttons.Set(PlayerInputType.LOCK_ON, buttonLockOn[i]);
				playerInput.buttons.Set(PlayerInputType.ABILITY_1, buttonAbility1[i]);
				playerInput.buttons.Set(PlayerInputType.ABILITY_2, buttonAbility2[i]);
				playerInput.buttons.Set(PlayerInputType.ABILITY_3, buttonAbility3[i]);
				playerInput.buttons.Set(PlayerInputType.ABILITY_4, buttonAbility4[i]);
				playerInput.buttons.Set(PlayerInputType.EXTRA_1, buttonExtra1[i]);
				playerInput.buttons.Set(PlayerInputType.EXTRA_2, buttonExtra2[i]);
				playerInput.buttons.Set(PlayerInputType.EXTRA_3, buttonExtra3[i]);
				playerInput.buttons.Set(PlayerInputType.EXTRA_4, buttonExtra4[i]);

				frameworkInput.players.Set(i, playerInput);
            }

			// Hand over the data to Fusion
			input.Set(frameworkInput);
		}

		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

		public override void FixedUpdateNetwork()
		{
			inputBufferPosition++;
			if (GetInput(out NetworkClientInputData input))
			{
				inputBuffer.Set((inputBufferPosition + setInputDelay) % inputBuffer.Length, input);
				RPC_SendInputToServer(inputBufferPosition + setInputDelay, input);
				latestConfirmedInput = inputBufferPosition + setInputDelay;
			}
		}

		[Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.StateAuthority)]
		private void RPC_SendInputToServer(int index, NetworkClientInputData input)
		{
			latestConfirmedInput = index;
			inputBuffer.Set(index % inputBuffer.Length, input);
		}

		public NetworkPlayerInputData GetInput(int inputIndex)
		{
			return inputBufferPosition <= latestConfirmedInput
				? inputBuffer[inputBufferPosition % inputBuffer.Length].players[inputIndex]
				: inputBuffer[latestConfirmedInput % inputBuffer.Length].players[inputIndex];
		}

		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
		public void OnConnectedToServer(NetworkRunner runner) { }
		public void OnDisconnectedFromServer(NetworkRunner runner) { }
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
		public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
	}
}