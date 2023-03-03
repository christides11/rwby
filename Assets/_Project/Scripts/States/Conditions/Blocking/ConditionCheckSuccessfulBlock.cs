using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCheckSuccessfulBlock : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public int checkDistance;
        public bool inverse;

        public IConditionVariables Copy()
        {
            return new ConditionCheckSuccessfulBlock()
            {
                checkDistance = checkDistance,
                inverse = inverse
            };
        }
    }
}