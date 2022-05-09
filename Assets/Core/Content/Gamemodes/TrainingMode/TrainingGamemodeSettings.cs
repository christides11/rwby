using Fusion;

namespace rwby.core.training
{
    [System.Serializable]
    public struct TrainingGamemodeSettings : INetworkStruct
    {
        public NetworkModObjectGUIDReference map;
    }
}