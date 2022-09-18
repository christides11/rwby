using HnSF;
using UnityEngine.Scripting.APIUpdating;

namespace rwby
{
    [System.Serializable]
    [MovedFrom(true, "rwby", "rwby.csharp", sourceClassName: "ConditionHitCount")]
    public struct ConditionHitOrBlockCount : IConditionVariables
    {
        [System.Serializable]
        [System.Flags]
        public enum HitOrBlockEnum
        {
            Hit = 1 << 0,
            Block = 1 << 1
        }

        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
        public int hitCount;

        public HitOrBlockEnum hitOrBlock;
    }
}