using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionButtonHeld : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;
        
        public bool inverse;
        public PlayerInputType button;
        public int offset;
        public int holdTime;
    }
}