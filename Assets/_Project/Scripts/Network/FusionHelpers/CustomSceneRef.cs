using Fusion;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public struct CustomSceneRef : INetworkStruct
    {
        // 0 = internal
        public byte source;
        public uint modIdentifier;
        public byte mapIdentifier;
        public sbyte sceneIdentifier;

        public override string ToString()
        {
            return $"{source}:{modIdentifier}:{mapIdentifier} scene {sceneIdentifier}";
        }
    }
}