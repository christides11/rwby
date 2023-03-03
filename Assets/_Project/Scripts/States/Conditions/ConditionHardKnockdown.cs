using System.Collections;
using System.Collections.Generic;
using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionHardKnockdown : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;

        public IConditionVariables Copy()
        {
            return new ConditionHardKnockdown()
            {
                inverse = inverse,
            };
        }
    }
}