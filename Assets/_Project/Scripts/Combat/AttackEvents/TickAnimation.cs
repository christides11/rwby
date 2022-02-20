using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class TickAnimation : HnSF.Combat.AttackEvent
    {
        public int frameAdvance = 1;
        public bool ignoreHitstop = false;

        public override string GetName()
        {
            return "Tick Animation";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager manager = (FighterManager)controller;
            if (ignoreHitstop == false && manager.FCombatManager.HitStop > 0) return AttackEventReturnType.NONE;
            manager.fighterAnimator.currentFrame = manager.fighterAnimator.currentFrame + frameAdvance;
            return AttackEventReturnType.NONE;
        }
    }
}