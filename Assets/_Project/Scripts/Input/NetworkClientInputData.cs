using Fusion;
using UnityEngine;

namespace rwby
{
	[System.Serializable]
	public struct NetworkClientInputData : INetworkInput
	{
		[Networked, Capacity(4)] public NetworkArray<NetworkPlayerInputData> players => default;
	}

	[System.Serializable]
	public struct NetworkPlayerInputData : INetworkStruct
    {
		public NetworkButtons buttons;
		public Vector2 movement;
		public Vector3 forward;
		public Vector3 right;
    }
}