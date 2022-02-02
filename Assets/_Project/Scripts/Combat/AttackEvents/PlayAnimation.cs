using HnSF.Combat;
using HnSF.Fighters;

namespace rwby.Combat.AttackEvents
{
    public class PlayAnimation : HnSF.Combat.AttackEvent
    {
        public string animationbank;
        public string animationName;

        public override string GetName()
        {
            return "Play Animation";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager manager = (FighterManager)controller;
            manager.fighterAnimator.Play(animationbank, animationName);
            return AttackEventReturnType.NONE;
        }
    }
}