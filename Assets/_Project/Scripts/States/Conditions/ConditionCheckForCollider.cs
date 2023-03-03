using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCheckForCollider : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
        public LayerMask layerMask;
        public Vector3 offset;
        public Vector3 halfExtents;

        public IConditionVariables Copy()
        {
            return new ConditionCheckForCollider()
            {
                inverse = inverse,
                layerMask = layerMask
            };
        }
    }
}