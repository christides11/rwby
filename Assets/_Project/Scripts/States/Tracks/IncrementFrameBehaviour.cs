using HnSF.Sample.TDAction;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class IncrementFrameBehaviour : FighterStateBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager fm = (FighterManager)playerData;
            if (fm == null) return;
            fm.StateManager.IncrementFrame();
        }
    }
}