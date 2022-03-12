using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class RotationBehaviour : FighterStateBehaviour
    {
        public ForceSetType rotationSetType;
        public Vector3 euler;
    }
}