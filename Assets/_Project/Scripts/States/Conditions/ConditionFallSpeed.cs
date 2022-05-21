using System.Collections;
using System.Collections.Generic;
using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionFallSpeed : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.FALL_SPEED;

        public bool absoluteValue;
        public float minValue;
        public float maxValue;
    }
}