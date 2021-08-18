using Fusion;
using Fusion.Sockets;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
	[OrderBefore(typeof(FighterInputManager), typeof(FighterManager))]
    public class ClientManager : NetworkBehaviour, INetworkRunnerCallbacks
    {
		public delegate void EmptyAction();
		public static event EmptyAction OnStartHosting;

		public static ClientManager local;

		public static List<ClientManager> clientManagers = new List<ClientManager>();

		[Networked] public string PlayerName { get; set; }
		[Networked, Capacity(50)] public string SelectedCharacter { get; set; }
		[Networked] public NetworkBool LobbyReadyStatus { get; set; }
		[Networked] public NetworkBool MatchReadyStatus { get; set; }

		[SerializeField] protected FighterInputManager inputManager;

		protected NetworkManager networkManager;

		Rewired.Player p = null;

        public void Awake()
        {
			networkManager = NetworkManager.singleton;
			MatchManager.onMatchSettingsLoadFailed += MatchSettingsLoadFail;
			MatchManager.onMatchSettingsLoadSuccess += MatchSettingsLoadSuccess;
			DontDestroyOnLoad(gameObject);
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

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void RPC_SetPlayer(FighterInputManager player)
        {
			inputManager = player;
        }

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
			frameworkInput.forward = Vector3.forward;
			frameworkInput.right = Vector3.right;
			if (p.GetButton(Action.Jump)) { frameworkInput.Buttons |= NetworkInputData.BUTTON_JUMP; }
			if (p.GetButton(Action.Light_Attack)) { frameworkInput.Buttons |= NetworkInputData.BUTTON_LIGHT_ATTACK; }
			if (p.GetButton(Action.Heavy_Attack)) { frameworkInput.Buttons |= NetworkInputData.BUTTON_HEAVY_ATTACK; }
			if (p.GetButton(Action.Block)) { frameworkInput.Buttons |= NetworkInputData.BUTTON_BLOCK; }
			if (p.GetButton(Action.Dash)) { frameworkInput.Buttons |= NetworkInputData.BUTTON_DASH; }
			if (p.GetButton(Action.Shoot)) { frameworkInput.Buttons |= NetworkInputData.BUTTON_SHOOT; }
			if (p.GetButton(Action.Lock_On)) { frameworkInput.Buttons |= NetworkInputData.BUTTON_LOCK_ON; }
			// Hand over the data to Fusion
			input.Set(frameworkInput);
		}

		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
		{
		}

		public override void FixedUpdateNetwork()
		{
			// Get our input struct and act accordingly. This method will only return data if we
			// have Input or State Authority - meaning on the controlling player or the server.
			if (GetInput(out NetworkInputData input))
			{
				if (inputManager == null)
                {
					return;
                }
				inputManager.FeedInput(networkManager.FusionLauncher.NetworkRunner.Simulation.Tick, input);
			}
		}

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
		public void OnConnectedToServer(NetworkRunner runner) { }
		public void OnDisconnectedFromServer(NetworkRunner runner) { }
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request) { }
		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
		public void OnObjectWordsChanged(NetworkRunner runner, NetworkObject networkedObject, HashSet<int> changedWords, NetworkObjectMemoryPtr oldMemory) { }
	}
}