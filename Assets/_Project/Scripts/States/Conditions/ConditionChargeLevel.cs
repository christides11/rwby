using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionChargeLevel : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public int minLevel;
        public int maxLevel;
    }
}