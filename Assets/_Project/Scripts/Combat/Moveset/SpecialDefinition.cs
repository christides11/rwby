using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class SpecialDefinition
    {
        [SelectImplementation(typeof(FighterStateReferenceBase))] [SerializeField, SerializeReference]
        public FighterStateReferenceBase groundState;
        [SelectImplementation(typeof(FighterStateReferenceBase))] [SerializeField, SerializeReference]
        public FighterStateReferenceBase aerialState;
    }
}