using System.Collections;
using System.Collections.Generic;
using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionHasThrowees : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;
        
    }
}