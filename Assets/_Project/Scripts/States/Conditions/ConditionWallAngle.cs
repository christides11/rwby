using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionWallAngle : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public VarInputSourceType inputSource;
        public bool inverse;
        public float minAngle;
        public float maxAngle;

        public IConditionVariables Copy()
        {
            return new ConditionWallAngle()
            {
                inputSource = inputSource,
                inverse = inverse,
                minAngle = minAngle,
                maxAngle = maxAngle
            };
        }
    }
}