using HnSF;
using HnSF.Combat;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SnapECBBehaviour : FighterStateBehaviour
    {
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;
            manager.FPhysicsManager.SnapECB();
        }
    }
}