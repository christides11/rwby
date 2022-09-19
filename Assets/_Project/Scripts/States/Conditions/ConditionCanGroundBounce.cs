using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCanGroundBounce : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
    }
}