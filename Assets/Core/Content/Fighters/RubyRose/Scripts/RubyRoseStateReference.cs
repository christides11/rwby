using HnSF;

namespace rwby.core
{
    [System.Serializable]
    public class RubyRoseStateReference :  FighterStateReferenceBase
    {
        public RubyRoseStates state = RubyRoseStates.SCYTHE_5A;
        
        public override int GetState()
        {
            return (int)state;
        }
    }
}