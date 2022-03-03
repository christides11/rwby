using Fusion;

namespace rwby
{
    public struct CustomSceneRef : INetworkStruct
    {
        // 0 = internal
        public byte source;
        public uint modIdentifier;
        public byte sceneIndex;
    }
}