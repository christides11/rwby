using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;

namespace rwby.Combat.AttackEvents
{
    public class CreateEffect : AttackEvent
    {
        public string effectbankName;
        public string effectName;

        public Vector3 offset;
        public Vector3 rotationOffset;

        public override string GetName()
        {
            return "Create Effect";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            FighterManager fighterManager = (controller as FighterManager);

            Vector3 realOffset = fighterManager.transform.forward * offset.z
                + fighterManager.transform.right * offset.x
                + fighterManager.transform.up * offset.y;

            EffectbankContainer effectbankContainer = fighterManager.EffectbankContainer;
            BaseEffect effect = effectbankContainer.CreateEffect(fighterManager.transform.position + realOffset, fighterManager.transform.rotation * Quaternion.Euler(rotationOffset), 
                effectbankName, effectName);
            effect.PlayEffect(true);
            return AttackEventReturnType.NONE;
        }
    }
}