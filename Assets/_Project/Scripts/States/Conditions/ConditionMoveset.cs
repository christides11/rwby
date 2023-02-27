using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionMoveset : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public bool inverse;

        public int moveset;
    }
}