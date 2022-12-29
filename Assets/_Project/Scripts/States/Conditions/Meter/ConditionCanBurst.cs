using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCanBurst : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;
    }
}