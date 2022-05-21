using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionIsGrounded : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.IS_GROUNDED;

        public bool inverse;
    }
}