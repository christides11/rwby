using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class MovesetSwitch : HnSF.Combat.AttackEvent
    {
        public int moveset;

        public override string GetName()
        {
            return "Moveset Switch";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager e = (FighterManager)controller;

            e.FCombatManager.SetMoveset(moveset);
            return AttackEventReturnType.NONE;
        }
    }
}