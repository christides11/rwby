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
    }
}