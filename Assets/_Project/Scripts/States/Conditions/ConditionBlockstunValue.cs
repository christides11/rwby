using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionBlockstunValue : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public int minValue;
        public int maxValue;
    }
}