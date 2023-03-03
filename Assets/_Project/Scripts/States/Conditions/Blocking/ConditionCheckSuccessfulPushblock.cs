using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCheckSuccessfulPushblock : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public int checkLength;
        public bool inverse;

        public IConditionVariables Copy()
        {
            return new ConditionCheckSuccessfulPushblock()
            {
                checkLength = checkLength,
                inverse = inverse
            };
        }
    }
}