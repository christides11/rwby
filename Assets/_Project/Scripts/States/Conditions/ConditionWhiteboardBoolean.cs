using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionWhiteboardBoolean : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
        public int valueIndex;

        public IConditionVariables Copy()
        {
            return new ConditionWhiteboardBoolean()
            {
                inverse = inverse,
                valueIndex = valueIndex
            };
        }
    }
}