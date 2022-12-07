using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionInHitstun : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
    }
}