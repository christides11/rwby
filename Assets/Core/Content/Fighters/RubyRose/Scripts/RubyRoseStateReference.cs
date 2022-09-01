using HnSF;

namespace rwby.core
{
    [System.Serializable]
    public class RubyRoseStateReference :  FighterStateReferenceBase
    {
        public RubyRoseStates state = RubyRoseStates.GRD_5a;
        
        public override int GetState()
        {
            return (int)state;
        }
    }
}