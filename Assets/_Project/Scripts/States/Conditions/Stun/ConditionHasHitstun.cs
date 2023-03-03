using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionHasHitstun : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;

        public IConditionVariables Copy()
        {
            return new ConditionHasHitstun()
            {
                inverse = inverse
            };
        }
    }
}