using HnSF.Combat;
using HnSF.Fighters;

namespace rwby.Combat.AttackEvents
{
    public class PlaySFX : AttackEvent
    {
        public string sndbkName;
        public string sfxName;

        public override string GetName()
        {
            return "Play SFX";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            SoundbankContainer soundbankContainer = (controller as FighterManager).SoundbankContainer;
            soundbankContainer.PlaySound(sndbkName, sfxName);
            return AttackEventReturnType.NONE;
        }
    }
}