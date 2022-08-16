using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCompareSlopeDir : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public VarInputSourceType inputSource;
        public float maxAngle;
    }
}