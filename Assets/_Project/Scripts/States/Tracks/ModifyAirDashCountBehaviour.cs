using HnSF.Sample.TDAction;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ModifyAirDashCountBehaviour : FighterStateBehaviour
    {
        public ModifyValueType modify;
        public int amount = 1;
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager cm = playerData as FighterManager;
            if (conditon.IsTrue(cm) == false) return;
            switch (modify)
            {
                case ModifyValueType.ADD:
                    cm.CurrentAirDash += amount;
                    break;
                case ModifyValueType.SET:
                    cm.CurrentAirDash = amount;
                    break;
            }
        }
    }
}