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
    public class PlayerHUDElement : HUDElement
    {
        public Image healthBarBack;
        public Image healthBarFront;
        public Image auraBarFront;
        
        [ReadOnly] public int healthValue;
        [ReadOnly] public int maxHealthValue;
        [ReadOnly] public float lerpTime = 0.0f;
        [ReadOnly] public int auraValue;
        [ReadOnly] public int maxAuraValue;
        public float chipSpeed = 1.0f;
        public float chipDelay = 0.1f;

        public float healthMinFill;
        public float healthMaxFill;
        public float auraMinFill;
        public float auraMaxFill;
        
        public override void InitializeElement(BaseHUD parentHUD)
        {
            parentHUD.playerFighter.HealthManager.OnHealthDecreased += HealthDecreased;
            parentHUD.playerFighter.HealthManager.OnHealthIncreased += HealthIncreased;
            parentHUD.playerFighter.FCombatManager.OnAuraDecreased += AuraDecreased;
            parentHUD.playerFighter.FCombatManager.OnAuraIncreased += AuraIncreased;
            healthValue = parentHUD.playerFighter.HealthManager.Health;
            maxHealthValue = parentHUD.playerFighter.fighterDefinition.Health;
            auraValue = parentHUD.playerFighter.FCombatManager.Aura;
            maxAuraValue = parentHUD.playerFighter.fighterDefinition.Aura;
            lerpTime = -chipDelay;
        }
        
        private void HealthIncreased(HealthManager healthmanager)
        {
            healthValue = healthmanager.Health;
            lerpTime = -chipDelay;
        }

        private void HealthDecreased(HealthManager healthmanager)
        {
            healthValue = healthmanager.Health;
            lerpTime = -chipDelay;
        }
        
        private void AuraIncreased(FighterCombatManager combatManager, int maxAura)
        {
            auraValue = combatManager.Aura;
            maxAuraValue = maxAura;
        }

        private void AuraDecreased(FighterCombatManager combatManager, int maxAura)
        {
            auraValue = combatManager.Aura;
            maxAuraValue = maxAura;
        }

        public override void UpdateElement(BaseHUD parentHUD)
        {
            UpdateHealthbar();
            UpdateAurabar();
        }

        private void UpdateHealthbar()
        {
            float fillF = healthBarFront.fillAmount;
            float fillB = healthBarBack.fillAmount;
            float hFraction = healthMinFill + ( (healthMaxFill-healthMinFill) * ((float)healthValue / (float)maxHealthValue ));
            if (fillB > hFraction)
            {
                healthBarFront.fillAmount = hFraction;
                healthBarBack.color = Color.red;
                lerpTime += Time.deltaTime;
                float percentComplete = Mathf.Clamp(lerpTime, 0, float.MaxValue) / chipSpeed;
                percentComplete *= percentComplete;
                healthBarBack.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
            }

            if (fillF < hFraction)
            {
                healthBarBack.fillAmount = hFraction;
                healthBarBack.color = Color.grey;
                lerpTime += Time.deltaTime;
                float percentComplete = Mathf.Clamp(lerpTime, 0, float.MaxValue) / chipSpeed;
                percentComplete *= percentComplete;
                healthBarFront.fillAmount = Mathf.Lerp(fillF, hFraction, percentComplete);
            }
        }

        private void UpdateAurabar()
        {
            auraBarFront.fillAmount = auraMinFill + ( (auraMaxFill-auraMinFill) * ((float)auraValue / (float)maxAuraValue) );
        }
    }
}