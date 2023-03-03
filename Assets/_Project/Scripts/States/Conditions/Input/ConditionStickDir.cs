using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionStickDir : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.BUTTON;
        
        public Vector2 stickDirection;
        public float directionDeviation;
        public int framesBack;
        public bool inverse;

        public IConditionVariables Copy()
        {
            return new ConditionStickDir()
            {
                stickDirection = stickDirection,
                directionDeviation = directionDeviation,
                framesBack = framesBack,
                inverse = inverse
            };
        }
    }
}