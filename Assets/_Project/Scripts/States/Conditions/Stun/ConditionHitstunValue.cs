using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionHitstunValue : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public int minValue;
        public int maxValue;

        public IConditionVariables Copy()
        {
            return new ConditionHitstunValue()
            {
                minValue = minValue,
                maxValue = maxValue
            };
        }
    }
}