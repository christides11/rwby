using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class ThrowInfo
    {
        public bool airOnly;
        public bool groundOnly;
        public float damageOnGrab;
        
        [SelectImplementation((typeof(FighterStateReferenceBase)))] [SerializeReference]
        public FighterStateReferenceBase confirmState = new FighterCmnStateReference();
    }
}