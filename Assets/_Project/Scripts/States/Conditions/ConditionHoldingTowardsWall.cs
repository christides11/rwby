using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionHoldingTowardsWall : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public int buffer;
        public bool inverse;
    }
}