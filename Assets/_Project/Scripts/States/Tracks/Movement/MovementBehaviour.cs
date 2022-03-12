using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class MovementBehaviour : FighterStateBehaviour
    {
        public ForceSetType forceSetType;
        public Vector3 force = Vector3.zero;
    }
}