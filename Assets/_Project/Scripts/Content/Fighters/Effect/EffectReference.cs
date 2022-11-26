using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct EffectReference
    {
        public ModObjectSetContentReference effectbank;
        public string effect;
        [SelectImplementation(typeof(FighterBoneReferenceBase))] [SerializeField, SerializeReference]
        public FighterBoneReferenceBase parent;
        public Vector3 offset;
        public Vector3 rotation;
        public Vector3 scale;
        public bool autoIncrement;
    }
}