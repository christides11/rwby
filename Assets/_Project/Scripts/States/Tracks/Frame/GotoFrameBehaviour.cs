using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class GotoFrameBehaviour : FighterStateBehaviour
    {
        public int frame = 1;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager cm = playerData as FighterManager;
            cm.StateManager.SetFrame(frame);
        }
    }
}