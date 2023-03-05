using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionMoveset : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;

        public bool checkCurrentMovesetInstead;
        public int moveset;

        public IConditionVariables Copy()
        {
            return new ConditionMoveset()
            {
                inverse = inverse,
                checkCurrentMovesetInstead = checkCurrentMovesetInstead,
                moveset = moveset
            };
        }
    }
}