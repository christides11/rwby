using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionLockedOn : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
        public bool requireTarget;
    }
}