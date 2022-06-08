using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct EffectReference
    {
        public ModObjectGUIDReference effectbank;
        public string effect;
        public bool parented;
        public Vector3 offset;
        public Vector3 rotation;
    }
}