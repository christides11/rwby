using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionAuraPercentage : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        [Range(0.0f, 1.0f)] public float percentage;
        public bool inverse;
    }
}