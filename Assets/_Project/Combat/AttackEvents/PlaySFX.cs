using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class PlaySFX : AttackEvent
    {
        public string sndbkName;
        public string sfxName;
        public float volume = 1.0f;
        [Range(0, 100)] public int playChance = 100;

        public override string GetName()
        {
            return "Play SFX";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager fm = (controller as FighterManager);
            if ((playChance != 100 && playChance == 0) || fm.matchManager.GetIntRangeInclusive(1, 100) >= playChance)
            {
                return AttackEventReturnType.NONE;
            }
            SoundbankContainer soundbankContainer = fm.SoundbankContainer;
            soundbankContainer.PlaySound(sndbkName, sfxName, volume);
            return AttackEventReturnType.NONE;
        }
    }
}