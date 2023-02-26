namespace Fusion.KCC
{
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct KCCNetworkID
	{
		// PUBLIC MEMBERS

		[FieldOffset(0)]
		public int  A;
		[FieldOffset(4)]
		public int  B;
		[FieldOffset(8)]
		public int  C;
		[FieldOffset(12)]
		public int  D;
		[FieldOffset(0)]
		public uint Raw;
		[FieldOffset(0)]
		public long Value0;
		[FieldOffset(8)]
		public long Value1;

		public bool IsValid => (A | B | C | D) != default;

		// PUBLIC METHODS

		public bool Equals(KCCNetworkID other) => A == other.A && B == other.B && C == other.C && D == other.D;

		public static KCCNetworkID GetNetworkID(NetworkObject networkObject)
		{
			if (networkObject == null)
				return default;

			KCCNetworkID networkID = new KCCNetworkID();

			if (networkObject.Id.IsValid == true)
			{
				networkID.Raw = networkObject.Id.Raw;
			}
			else
			{
				networkID.Value0 = networkObject.NetworkGuid.RawGuidValue[0];
				networkID.Value1 = networkObject.NetworkGuid.RawGuidValue[1];
			}

			return networkID;
		}

		public static NetworkObject GetNetworkObject(NetworkRunner runner, KCCNetworkID networkID)
		{
			if (networkID.IsValid == false)
				return default;

			if (networkID.B == default && networkID.C == default && networkID.D == default)
			{
				NetworkId networkId = new NetworkId();
				networkId.Raw = networkID.Raw;
				NetworkObject networkObjectInstance = runner.FindObject(networkId);
				if (networkObjectInstance != null)
					return networkObjectInstance;
			}

			NetworkObjectGuid networkObjectGuid = new NetworkObjectGuid();
			networkObjectGuid.RawGuidValue[0] = networkID.Value0;
			networkObjectGuid.RawGuidValue[1] = networkID.Value1;

			if (runner.Config.PrefabTable.TryGetId(networkObjectGuid, out NetworkPrefabId networkPrefabId) == true && runner.Config.PrefabTable.TryGetPrefab(networkPrefabId, out NetworkObject networkObject) == true)
				return networkObject;

			return default;
		}

		public override string ToString()
		{
			return $"{A} | {B} | {C} | {B}";
		}
	}
}
