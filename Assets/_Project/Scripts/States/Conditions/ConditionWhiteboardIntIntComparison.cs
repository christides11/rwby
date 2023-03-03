using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionWhiteboardIntIntComparison : IConditionVariables
    {
        public enum ComparisonTypes
        {
            LESS_THAN,
            LESS_THAN_OR_EQUAL,
            EQUAL,
            GREATER_THAN,
            GREATER_THAN_OR_EQUAL
        }
        
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
        public ComparisonTypes comparison;
        public int valueAIndex;
        public int valueBIndex;

        public IConditionVariables Copy()
        {
            return new ConditionWhiteboardIntIntComparison()
            {
                inverse = inverse,
                comparison = comparison,
                valueAIndex = valueAIndex,
                valueBIndex = valueBIndex
            };
        }
    }
}