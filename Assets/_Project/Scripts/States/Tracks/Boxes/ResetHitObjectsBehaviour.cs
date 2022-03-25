using HnSF;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ResetHitObjectsBehaviour : FighterStateBehaviour
    {

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;
            manager.FCombatManager.HitboxManager.hitObjects.Clear();
        }
    }
}