using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCanWallBounce : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;

        public IConditionVariables Copy()
        {
            return new ConditionCanWallBounce()
            {
                inverse = inverse
            };
        }
    }
}