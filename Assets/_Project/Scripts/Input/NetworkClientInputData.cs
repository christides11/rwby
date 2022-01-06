using Fusion;
using UnityEngine;

namespace rwby
{
	public struct NetworkClientInputData : INetworkInput
	{
		public NetworkPlayerInputData player1;
		public NetworkPlayerInputData player2;
		public NetworkPlayerInputData player3;
		public NetworkPlayerInputData player4;
	}

	public struct NetworkPlayerInputData : INetworkStruct
    {
		public NetworkButtons buttons;
		public Vector2 movement;
		public Vector3 forward;
		public Vector3 right;
    }
}