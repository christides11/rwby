using HnSF;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class DecrementHitstunBehaviour : FighterStateBehaviour
    {

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;
            if(manager.CombatManager.HitStun > 0) manager.CombatManager.AddHitStun(-1);
        }
    }
}