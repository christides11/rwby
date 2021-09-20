using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace rwby
{
    public class PlayerHUD : BaseHUD
    {
        [SerializeField] protected Image healthFill;
        [SerializeField] protected Image healthRedFill;
        [SerializeField] protected TextMeshProUGUI stateText;
        [SerializeField] protected TextMeshProUGUI speedText;
        [SerializeField] protected TextMeshProUGUI gravityText;
        [SerializeField] protected TextMeshProUGUI rttText;

        public override void Update()
        {
            if (client.inMan == null) return;

            stateText.text = client.inMan.manager.StateManager.GetCurrentStateName();
            speedText.text = client.inMan.manager.PhysicsManager.forceMovement.magnitude.ToString("F1");
            gravityText.text = client.inMan.manager.PhysicsManager.forceGravity.ToString("F1");
            rttText.text = (client.Runner.GetPlayerRtt(client.Runner.LocalPlayer) * 1000).ToString("F0");
        }
    }
}