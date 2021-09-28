using Fusion;
using Fusion.Sockets;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
	[OrderBefore(typeof(FighterInputManager), typeof(FighterManager))]
	public class ClientManager : NetworkBehaviour, INetworkRunnerCallbacks, IBeforeUpdate, IAfterUpdate
	{
		public delegate void EmptyAction();
		public static event EmptyAction OnStartHosting;

		public static ClientManager local;

		public static List<ClientManager> clientManagers = new List<ClientManager>();

		[Networked] public string PlayerName { get; set; }
		[Networked, Capacity(50)] public string SelectedCharacter { get; set; }
		[Networked] public NetworkBool LobbyReadyStatus { get; set; }
		[Networked] public NetworkBool MatchReadyStatus { get; set; }

		[Networked] public NetworkObject ClientFighter { get; set; }

		public FighterInputManager inMan;

		protected NetworkManager networkManager;

		Rewired.Player p = null;

		public PlayerCamera camera;

		protected virtual void Awake()
		{
			networkManager = NetworkManager.singleton;
			MatchManager.onMatchSettingsLoadFailed += MatchSettingsLoadFail;
			MatchManager.onMatchSettingsLoadSuccess += MatchSettingsLoadSuccess;
			DontDestroyOnLoad(gameObject);
		}

		public override void Render()
		{
			if (camera)
			{
				camera.CamUpdate();
			}
		}

		bool buttonJump;
		bool buttonLightAttack;
		bool buttonHeavyAttack;
		bool buttonBlock;
		bool buttonDash;
		bool buttonGrab;
		bool buttonLockOn;
		bool buttonAbility1;
		bool buttonAbility2;
		bool buttonAbility3;
		bool buttonAbility4;
		bool buttonExtra1;
		bool buttonExtra2;
		bool buttonExtra3;
		bool buttonExtra4;
		public void BeforeUpdate()
		{
			if (p != null)
			{
				if (p.GetButton(Action.Jump)) { buttonJump = true; }
				if (p.GetButton(Action.Light_Attack)) { buttonLightAttack = true; }
				if (p.GetButton(Action.Heavy_Attack)) { buttonHeavyAttack = true; }
				if (p.GetButton(Action.Block)) { buttonBlock = true; }
				if (p.GetButton(Action.Dash)) { buttonDash = true; }
				if (p.GetButton(Action.Grab)) { buttonGrab = true; }
				if (p.GetButton(Action.Lock_On)) { buttonLockOn = true; }
				if (p.GetButton(Action.Ability_1)) { buttonAbility1 = true; }
				if (p.GetButton(Action.Ability_2)) { buttonAbility2 = true; }
				if (p.GetButton(Action.Ability_3)) { buttonAbility3 = true; }
				if (p.GetButton(Action.Ability_4)) { buttonAbility4 = true; }
				if (p.GetButton(Action.Extra1)) { buttonExtra1 = true; }
				if (p.GetButton(Action.Extra2)) { buttonExtra2 = true; }
				if (p.GetButton(Action.Extra3)) { buttonExtra3 = true; }
				if (p.GetButton(Action.Extra4)) { buttonExtra4 = true; }
			}
		}

		public void AfterUpdate()
		{
			ClearInputs();
		}

		protected virtual void ClearInputs()
		{
			buttonJump = false;
			buttonLightAttack = false;
			buttonHeavyAttack = false;
			buttonBlock = false;
			buttonDash = false;
			buttonGrab = false;
			buttonLockOn = false;
			buttonAbility1 = false;
			buttonAbility2 = false;
			buttonAbility3 = false;
			buttonAbility4 = false;
			buttonExtra1 = false;
			buttonExtra2 = false;
			buttonExtra3 = false;
			buttonExtra4 = false;
		}

		private void GamemodeSetupSuccess()
		{
			RPC_SetMatchReadyStatus(true);
		}

		private void GamemodeSetupFailure()
		{
			RPC_SetMatchReadyStatus(false);
		}

		public override void Spawned()
		{
			clientManagers.Add(this);
			if (Object.HasInputAuthority)
			{
				SetControllerID(0);
				GameModeBase.OnSetupFailure += GamemodeSetupFailure;
				GameModeBase.OnSetupSuccess += GamemodeSetupSuccess;
				local = this;
				Runner.AddCallbacks(this);
				RPC_Configure(GameManager.singleton.localUsername);
				BaseHUD bhud = GameObject.Instantiate(GameManager.singleton.settings.baseUI, transform, false);
				bhud.SetClient(this);
			}
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			base.Despawned(runner, hasState);
			clientManagers.Remove(this);
		}

		private void MatchSettingsLoadSuccess()
		{
			if (Object.HasStateAuthority)
			{
				LobbyReadyStatus = false;
			}
			if (Object.HasInputAuthority)
			{
				RPC_ReportReadyStatus(true);
			}
		}

		private void MatchSettingsLoadFail(MatchSettingsLoadFailedReason reason)
		{
			if (Object.HasStateAuthority)
			{
				LobbyReadyStatus = false;
			}
			if (Object.HasInputAuthority)
			{
				Debug.Log($"Match settings failed to load: {reason}");
				RPC_ReportReadyStatus(false);
			}
		}

		[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
		private void RPC_SetMatchReadyStatus(NetworkBool status)
		{
			MatchReadyStatus = status;
		}

		[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
		public void RPC_Configure(string name)
		{
			PlayerName = name;
		}

		[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
		public void RPC_SetCharacter(string characterIdentifier)
		{
			SelectedCharacter = characterIdentifier;
		}

		[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
		public void RPC_ReportReadyStatus(NetworkBool readyToPlay)
		{
			LobbyReadyStatus = readyToPlay;
		}

		public virtual void SetControllerID(int controllerID)
		{
			p = ReInput.players.GetPlayer(controllerID);
		}

		/*
		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void RPC_SetPlayer(NetworkObject player)
        {
			inputManager = player.GetComponent<FighterInputManager>();
        }*/

		/// <summary>
		/// Get Unity input and store them in a struct for Fusion
		/// </summary>
		/// <param name="runner">The current NetworkRunner</param>
		/// <param name="input">The target input handler that we'll pass our data to</param>
		public void OnInput(NetworkRunner runner, NetworkInput input)
		{
			// Instantiate our custom input structure
			var frameworkInput = new NetworkInputData();
			// Fill it with input data
			if (p == null)
			{
				input.Set(frameworkInput);
				return;
			}
			frameworkInput.movement = p.GetAxis2D(Action.Movement_X, Action.Movement_Y);
			if (camera)
			{
				frameworkInput.forward = camera.transform.forward;
				frameworkInput.right = camera.transform.right;
			}
			else
			{
				frameworkInput.forward = Vector3.forward;
				frameworkInput.right = Vector3.right;
			}
			if (buttonJump) { frameworkInput.Buttons |= NetworkInputData.BUTTON_JUMP; }
			if (buttonLightAttack) { frameworkInput.Buttons |= NetworkInputData.BUTTON_LIGHT_ATTACK; }
			if (buttonHeavyAttack) { frameworkInput.Buttons |= NetworkInputData.BUTTON_HEAVY_ATTACK; }
			if (buttonBlock) { frameworkInput.Buttons |= NetworkInputData.BUTTON_BLOCK; }
			if (buttonDash) { frameworkInput.Buttons |= NetworkInputData.BUTTON_DASH; }
			if (buttonGrab) { frameworkInput.Buttons |= NetworkInputData.BUTTON_GRAB; }
			if (buttonLockOn) { frameworkInput.Buttons |= NetworkInputData.BUTTON_LOCK_ON; }
			if (buttonAbility1) { frameworkInput.Buttons |= NetworkInputData.BUTTON_ABILITY_ONE; }
			if (buttonAbility2) { frameworkInput.Buttons |= NetworkInputData.BUTTON_ABILITY_TWO; }
			if (buttonAbility3) { frameworkInput.Buttons |= NetworkInputData.BUTTON_ABILITY_THREE; }
			if (buttonAbility4) { frameworkInput.Buttons |= NetworkInputData.BUTTON_ABILITY_FOUR; }
			if (buttonExtra1) { frameworkInput.Buttons |= NetworkInputData.BUTTON_Extra_1; }
			if (buttonExtra2) { frameworkInput.Buttons |= NetworkInputData.BUTTON_Extra_2; }
			if (buttonExtra3) { frameworkInput.Buttons |= NetworkInputData.BUTTON_Extra_3; }
			if (buttonExtra4) { frameworkInput.Buttons |= NetworkInputData.BUTTON_Extra_4; }
			// Hand over the data to Fusion
			input.Set(frameworkInput);
		}

		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
		{
		}

		[Networked] public NetworkInputData latestConfirmedInput { get; set; }

		[Networked, Capacity(10)] public NetworkArray<NetworkInputData> inputBuffer { get; set; }
		[Networked] public int inputBufferPosition { get; set; }

		public int setInputBuffer = 3;
		[NonSerialized] private int inputBufferCapacity = 10;

		public override void FixedUpdateNetwork()
		{

			// Get our input struct and act accordingly. This method will only return data if we
			// have Input or State Authority - meaning on the controlling player or the server.
			if (GetInput(out NetworkInputData input))
			{
				inputBuffer.Set((inputBufferPosition+setInputBuffer)%(inputBufferCapacity), input);
			}

			if (inMan == null)
			{
				if (ClientFighter != null)
				{
					inMan = ClientFighter.GetComponent<FighterInputManager>();
				}
				else
				{
					return;
				}
			}

			inMan.FeedInput(networkManager.FusionLauncher.NetworkRunner.Simulation.Tick, inputBuffer[(inputBufferPosition)%inputBufferCapacity]);
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
		public void OnObjectWordsChanged(NetworkRunner runner, NetworkObject networkedObject, HashSet<int> changedWords, NetworkObjectMemoryPtr oldMemory) { }
		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    }
}