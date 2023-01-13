using Fusion;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public struct CustomSceneRef : INetworkStruct
    {
        [FormerlySerializedAs("mapReference")] public NetworkModObjectGUIDReference mapContentReference;
        public sbyte sceneIdentifier;

        public CustomSceneRef(ModIDContentReference mapContentReference, sbyte sceneID)
        {
            this.mapContentReference = mapContentReference;
            sceneIdentifier = sceneID;
        }
        
        public CustomSceneRef(NetworkModObjectGUIDReference mapContentReference, sbyte sceneID)
        {
            this.mapContentReference = mapContentReference;
            sceneIdentifier = sceneID;
        }

        public CustomSceneRef(uint modGUID, int contentIdx, sbyte sceneID)
        {
            mapContentReference = new NetworkModObjectGUIDReference()
                { modGUID = modGUID, contentType = (int)ContentType.Map, contentIdx = (ushort)contentIdx };
            sceneIdentifier = sceneID;
        }
        
        public override string ToString()
        {
            return $"{mapContentReference.ToString()} scene {sceneIdentifier}";
        }
    }
}