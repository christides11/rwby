using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionHitboxHitCount : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
        public int hitboxIndex;
        public int hitCount;
    }
}