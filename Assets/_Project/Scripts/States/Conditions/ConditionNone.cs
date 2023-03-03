using System.Collections;
using System.Collections.Generic;
using HnSF;
using UnityEngine;

namespace rwby
{
    public struct ConditionNone : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public IConditionVariables Copy()
        {
            return new ConditionNone();
        }
    }
}