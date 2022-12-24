using Fusion;
using HnSF.Combat;
using UnityEngine;

namespace rwby
{
    public struct EntityBoxDefinition : INetworkStruct
    {
        public BoxShape shape;
        public Vector3 offset;
        public Vector3 extents;
    }
}