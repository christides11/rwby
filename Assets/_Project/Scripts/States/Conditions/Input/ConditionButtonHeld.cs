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

        public IConditionVariables Copy()
        {
            return new ConditionButtonHeld()
            {
                inverse = inverse,
                button = button,
                offset = offset,
                holdTime = holdTime
            };
        }
    }
}