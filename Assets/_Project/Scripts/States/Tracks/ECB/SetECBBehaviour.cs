using HnSF;
using HnSF.Combat;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SetECBBehaviour : FighterStateBehaviour
    {
        public float ecbCenter;
        public float ecbRadius;
        public float ecbHeight;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;
            manager.FPhysicsManager.SetECB(ecbCenter, ecbRadius, ecbHeight);
        }
    }
}