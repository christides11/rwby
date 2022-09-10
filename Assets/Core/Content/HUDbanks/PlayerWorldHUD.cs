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
    public class PlayerWorldHUD : MonoBehaviour
    {
        [HideInInspector] public FighterManager fighter;
        public Image healthBarBack;
        public Image healthBarFront;
        public Image aurabar;
        public Image hitstunbar;

        [ReadOnly] public int healthValue;
        [ReadOnly] public int maxHealthValue;
        [ReadOnly] public float lerpTime = 0.0f;
        [ReadOnly] public int auraValue;
        [ReadOnly] public int maxAuraValue;
        public float chipSpeed = 1.0f;
        public float chipDelay = 0.1f;
        
        public virtual void Setup(FighterManager fighter)
        {
            this.fighter = fighter;
            fighter.HealthManager.OnHealthDecreased += HealthDecreased;
            fighter.HealthManager.OnHealthIncreased += HealthIncreased;
            fighter.FCombatManager.OnAuraDecreased += AuraDecreased;
            fighter.FCombatManager.OnAuraIncreased += AuraIncreased;
            healthValue = fighter.HealthManager.Health;
            maxHealthValue = fighter.fighterDefinition.Health;
        }
        
        public virtual void UpdateHUD()
        {
            UpdateHealthbar();
            UpdateAurabar();
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
        
        private void UpdateHealthbar()
        {
            float fillF = healthBarFront.fillAmount;
            float fillB = healthBarBack.fillAmount;
            float hFraction = (float)healthValue / (float)maxHealthValue;
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
            aurabar.fillAmount = (float)auraValue / (float)maxAuraValue;
        }
    }
}