using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionPoleValid : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;

        public IConditionVariables Copy()
        {
            return new ConditionPoleValid()
            {
                inverse = inverse
            };
        }
    }
}