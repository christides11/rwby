using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCanEnemyStep : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;

        public IConditionVariables Copy()
        {
            return new ConditionCanEnemyStep()
            {
                inverse = inverse,
            };
        }
    }
}