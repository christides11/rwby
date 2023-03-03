using System.Collections;
using System.Collections.Generic;
using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionExternal : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public ExternalCondition externalCondition;

        public IConditionVariables Copy()
        {
            return new ConditionExternal()
            {
                externalCondition = externalCondition,
            };
        }
    }
}