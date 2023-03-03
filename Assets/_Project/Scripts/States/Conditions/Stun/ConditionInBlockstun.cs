using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionInBlockstun : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;

        public IConditionVariables Copy()
        {
            return new ConditionInBlockstun()
            {
                inverse = inverse
            };
        }
    }
}