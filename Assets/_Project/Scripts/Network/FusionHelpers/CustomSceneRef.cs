using Fusion;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public struct CustomSceneRef : INetworkStruct
    {
        public NetworkModObjectGUIDReference mapReference;
        public sbyte sceneIdentifier;

        public CustomSceneRef(ModObjectGUIDReference mapReference, sbyte sceneID)
        {
            this.mapReference = mapReference;
            sceneIdentifier = sceneID;
        }
        
        public CustomSceneRef(NetworkModObjectGUIDReference mapReference, sbyte sceneID)
        {
            this.mapReference = mapReference;
            sceneIdentifier = sceneID;
        }

        public CustomSceneRef(ContentGUID modGUID, ContentGUID contentGUID, sbyte sceneID)
        {
            mapReference = new NetworkModObjectGUIDReference()
                { modGUID = modGUID, contentType = (int)ContentType.Map, contentGUID = contentGUID };
            sceneIdentifier = sceneID;
        }
        
        public override string ToString()
        {
            return $"{mapReference.ToString()} scene {sceneIdentifier}";
        }
    }
}