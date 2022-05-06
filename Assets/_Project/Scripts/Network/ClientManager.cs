using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
	[OrderBefore(typeof(FighterInputManager), typeof(FighterManager))]
	public class ClientManager : NetworkBehaviour, INetworkRunnerCallbacks, IBeforeUpdate, IAfterUpdate
	{
		public delegate void ClientAction(ClientManager clientManager);
		public static event ClientAction OnPlayerCountChanged;
		
		[Networked(OnChanged = nameof(OnClientPlayerCountChanged))] public uint ClientPlayerAmount { get; set; }
		//public PlayerCamera[] playerCameras = new PlayerCamera[4];

		private NetworkManager networkManager;
		private GameManager gameManager;

		private bool sessionHandlerSet = false;
		private FusionLauncher sessionHandler;

		// INPUT //
		[Networked] public NetworkClientInputData latestConfirmedInput { get; set; }
		[Networked, Capacity(10)] public NetworkArray<NetworkClientInputData> inputBuffer { get; }
		[Networked] public int inputBufferPosition { get; set; }
		public int setInputDelay = 3;
		
		[Networked] public float mapLoadPercent { get; set; }

		protected virtual void Awake()
		{
			gameManager = GameManager.singleton;
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
			
			/*
			for(int i = 0; i < playerCameras.Length; i++)
            {
				if (playerCameras[i] == null) continue;
				playerCameras[i].CamUpdate();
            }*/
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

		/*
		public NetworkObject SpawnPlayer(PlayerRef owner, int playerIndex, Vector3 spawnPosition)
		{
			return null;
			ModObjectReference characterReference = ClientPlayers[playerIndex].characterReference;
			IFighterDefinition fighterDefinition = ContentManager.singleton.GetContentDefinition<IFighterDefinition>(characterReference);

			int indexTemp = playerIndex;
			ClientManager tempCM = this;
			NetworkObject no = Runner.Spawn(fighterDefinition.GetFighter().GetComponent<NetworkObject>(), spawnPosition, Quaternion.identity, owner, 
				(a, b) =>
		        {
					b.gameObject.name = $"{b.Id}.{playerIndex} : {fighterDefinition.Name}";
					b.GetBehaviour<FighterCombatManager>().Team = tempCM.ClientPlayers[indexTemp].team;
					var list = ClientPlayers;
					ClientPlayerDefinition temp = list[indexTemp];
					temp.characterNetID = b.Id;
					list[indexTemp] = temp;
				});
			return no;
		}*/

        Vector2[] buttonMovement = new Vector2[8];
		Vector2[] buttonCamera = new Vector2[8];
		Vector3[] buttonCameraForward = new Vector3[8];
		Vector3[] buttonCameraRight = new Vector3[8];
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
				/*
				if (rewiredPlayers[j] == null) continue;

				if (rewiredPlayers[j].GetButtonDown(Action.Pause))
                {
                    if (PauseMenu.singleton.paused)
                    {
						PauseMenu.singleton.Close();
                    }
                    else
                    {
						PauseMenu.singleton.Open();
                    }
                }

				if (PauseMenu.singleton.paused) return;

				buttonMovement[j] = rewiredPlayers[j].GetAxis2D(Action.Movement_X, Action.Movement_Y);
				buttonCamera[j] = rewiredPlayers[j].GetAxis2D(Action.Camera_X, Action.Camera_Y);
				buttonCameraForward[j] = playerCameras[j] ? playerCameras[j].transform.forward : Vector3.forward;
				buttonCameraRight[j] = playerCameras[j] ? playerCameras[j].transform.right : Vector3.right;
				if (rewiredPlayers[j].GetButton(Action.Jump)) buttonJump[j] = true;
				if (rewiredPlayers[j].GetButton(Action.A)) buttonA[j] = true;
				if (rewiredPlayers[j].GetButton(Action.B)) buttonB[j] = true;
				if (rewiredPlayers[j].GetButton(Action.C)) buttonC[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Block)) buttonBlock[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Dash)) buttonDash[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Lock_On)) buttonLockOn[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Ability_1)) buttonAbility1[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Ability_2)) buttonAbility2[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Ability_3)) buttonAbility3[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Ability_4)) buttonAbility4[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Extra1)) buttonExtra1[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Extra2)) buttonExtra2[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Extra3)) buttonExtra3[j] = true;
				if (rewiredPlayers[j].GetButton(Action.Extra4)) buttonExtra4[j] = true;*/
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

				switch (i)
				{
					case 0:
						frameworkInput.player1 = playerInput;
						break;
					case 1:
						frameworkInput.player2 = playerInput;
						break;
					case 2:
						frameworkInput.player3 = playerInput;
						break;
					case 3:
						frameworkInput.player4 = playerInput;
						break;
				}
			}

			// Hand over the data to Fusion
			input.Set(frameworkInput);
		}

		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

		public override void FixedUpdateNetwork()
		{
			if (GetInput(out NetworkClientInputData input))
			{
				inputBuffer.Set((inputBufferPosition + setInputDelay) % (inputBuffer.Length), input);
			}

			/*
			for(int i = 0; i < ClientPlayers.Count; i++)
            {
				if (ClientPlayers[i].characterNetID.IsValid == false) continue;
				FighterInputManager cim = Runner.TryGetNetworkedBehaviourFromNetworkedObjectRef<FighterInputManager>(ClientPlayers[i].characterNetID);

				NetworkPlayerInputData playerInput;

                switch (i)
                {
					case 0:
						playerInput = inputBuffer[(inputBufferPosition) % inputBuffer.Length].player1;
						break;
					case 1:
						playerInput = inputBuffer[(inputBufferPosition) % inputBuffer.Length].player2;
						break;
					case 2:
						playerInput = inputBuffer[(inputBufferPosition) % inputBuffer.Length].player3;
						break;
					case 3:
						playerInput = inputBuffer[(inputBufferPosition) % inputBuffer.Length].player4;
						break;
					default:
						playerInput = new NetworkPlayerInputData();
						break;
                }

				cim.FeedInput(Runner.Simulation.Tick, playerInput);
			}*/
			inputBufferPosition++;
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