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

        public IConditionVariables Copy()
        {
            return new ConditionCompareSlopeDir()
            {
                inputSource = inputSource,
                maxAngle = maxAngle
            };
        }
    }
}