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
    public class HealthbarHUDElement : HUDElement
    {
        public Image backBar;
        public Image frontBar;
        
        [ReadOnly] public int healthValue;
        [ReadOnly] public int maxHealthValue;
        [ReadOnly] public float lerpTime = 0.0f;
        public float chipSpeed = 1.0f;
        public float chipDelay = 0.1f;

        public override void InitializeElement(BaseHUD parentHUD)
        {
            parentHUD.playerFighter.HealthManager.OnHealthDecreased += HealthDecreased;
            parentHUD.playerFighter.HealthManager.OnHealthIncreased += HealthIncreased;
            healthValue = parentHUD.playerFighter.HealthManager.Health;
            maxHealthValue = parentHUD.playerFighter.fighterDefinition.Health;
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

        public override void UpdateElement(BaseHUD parentHUD)
        {
            float fillF = frontBar.fillAmount;
            float fillB = backBar.fillAmount;
            float hFraction = (float)healthValue / (float)maxHealthValue;
            if (fillB > hFraction)
            {
                frontBar.fillAmount = hFraction;
                backBar.color = Color.red;
                lerpTime += Time.deltaTime;
                float percentComplete = Mathf.Clamp(lerpTime, 0, float.MaxValue) / chipSpeed;
                percentComplete *= percentComplete;
                backBar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
            }

            if (fillF < hFraction)
            {
                backBar.fillAmount = hFraction;
                backBar.color = Color.grey;
                lerpTime += Time.deltaTime;
                float percentComplete = Mathf.Clamp(lerpTime, 0, float.MaxValue) / chipSpeed;
                percentComplete *= percentComplete;
                frontBar.fillAmount = Mathf.Lerp(fillF, hFraction, percentComplete);
            }
        }
    }
}