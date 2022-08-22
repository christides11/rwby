using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCompareInputDir : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
        public VarInputSourceType inputSourceA;
        public VarInputSourceType inputSourceB;
        public float minAngle;
        public float maxAngle;
        public bool signedAngle;
    }
}