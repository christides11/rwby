using System.Collections;
using System.Collections.Generic;
using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionFloorAngle : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public float minAngle;
        public float maxAngle;
    }
}