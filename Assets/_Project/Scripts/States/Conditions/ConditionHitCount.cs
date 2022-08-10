using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionHitCount : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
        public int hitCount;
    }
}