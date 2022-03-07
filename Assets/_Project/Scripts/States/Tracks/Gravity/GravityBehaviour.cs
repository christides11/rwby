using HnSF.Sample.TDAction;

namespace rwby
{
    [System.Serializable]
    public class GravityBehaviour : FighterStateBehaviour
    {
        public ForceSetType forceSetType;
        public float force = 0;
    }
}