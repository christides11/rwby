using System.Collections;
using System.Collections.Generic;
using System.Text;
using rwby;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace rwby
{
    public class AuraHUDElement : HUDElement
    {
        public Image bar;

        [ReadOnly] public int auraValue;
        [ReadOnly] public int maxAuraValue;

        public override void InitializeElement(BaseHUD parentHUD)
        {
            parentHUD.playerFighter.FCombatManager.OnAuraDecreased += AuraDecreased;
            parentHUD.playerFighter.FCombatManager.OnAuraIncreased += AuraIncreased;
            maxAuraValue = parentHUD.playerFighter.fighterDefinition.Aura;
            auraValue = maxAuraValue;
        }
        
        private void AuraIncreased(FighterCombatManager combatManager)
        {
            auraValue = combatManager.Aura;
        }

        private void AuraDecreased(FighterCombatManager combatManager)
        {
            auraValue = combatManager.Aura;
        }

        public override void UpdateElement(BaseHUD parentHUD)
        {
            bar.fillAmount = (float)auraValue / (float)maxAuraValue;
        }
    }
}