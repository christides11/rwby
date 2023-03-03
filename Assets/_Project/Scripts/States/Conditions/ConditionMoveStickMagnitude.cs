using System.Collections;
using System.Collections.Generic;
using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionMoveStickMagnitude : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.MOVEMENT_STICK_MAGNITUDE;
        
        public float minValue;
        public float maxValue;

        public IConditionVariables Copy()
        {
            return new ConditionMoveStickMagnitude()
            {
                minValue = minValue,
                maxValue = maxValue,
            };
        }
    }
}