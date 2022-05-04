using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace rwby
{
    // TODO
    public class PlayerHUD : BaseHUD
    {
        /*
        private FighterManager fm;
        [SerializeField] protected Image healthFill;
        [SerializeField] protected Image healthRedFill;
        [SerializeField] protected TextMeshProUGUI stateText;
        [SerializeField] protected TextMeshProUGUI stateFrameText;
        [SerializeField] protected TextMeshProUGUI speedText;
        [SerializeField] protected TextMeshProUGUI gravityText;
        [SerializeField] protected TextMeshProUGUI rttText;

        protected MovingAverage rttMovingAverage = new MovingAverage(60);

        public override void Update()
        {
            if (client == null || client.ClientPlayers[playerIndex].characterNetID.IsValid == false) return;
            if (fm == null)
            {
                fm = client.Runner.TryGetNetworkedBehaviourFromNetworkedObjectRef<FighterManager>(client.ClientPlayers[playerIndex].characterNetID);
                fm.FStateManager.OnStateChanged += UpdateStateText;
            }
        }

        private void FixedUpdate()
        {
            if (client == null) return;
            rttMovingAverage.ComputeAverage((int)(client.Runner.GetPlayerRtt(client.Runner.LocalPlayer) * 1000));
            rttText.text = $"{rttMovingAverage.Average.ToString("F0")} +/- {rttMovingAverage.StandardDeviation().ToString("F0")}";
            if (fm == null) return;
            //stateFrameText.text = fm.FStateManager.CurrentStateFrame.ToString();
            speedText.text = fm.FPhysicsManager.forceMovement.magnitude.ToString("F1");
            gravityText.text = fm.FPhysicsManager.forceGravity.ToString("F1");
        }

        private void UpdateStateText(FighterStateManager fsm)
        {
            stateText.text = fsm.GetCurrentStateName();
        }*/
    }
}