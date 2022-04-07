using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct CustomSceneRef : INetworkStruct
    {
        // 0 = internal
        public byte source;
        public uint modIdentifier;
        public byte sceneIndex;
    }
}