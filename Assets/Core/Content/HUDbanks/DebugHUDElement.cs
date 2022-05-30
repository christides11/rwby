using System.Collections;
using System.Collections.Generic;
using System.Text;
using rwby;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    public class DebugHUDElement : HUDElement
    {
        public TextMeshProUGUI simulationFrame;
        public TextMeshProUGUI position;
        public TextMeshProUGUI stateFrame;
        public TextMeshProUGUI stateName;
        public TextMeshProUGUI velocityTotal;
        public TextMeshProUGUI velocityMovement;
        public TextMeshProUGUI speed;

        [FormerlySerializedAs("stickMaxRange")] public int stickRadius = 49;
        public RectTransform stickRect;

        public override void InitializeElement(BaseHUD parentHUD)
        {
            
        }

        public override void UpdateElement(BaseHUD parentHUD)
        {
            position.text = parentHUD.playerFighter.myTransform.position.ToString("F1");
            stateFrame.text = parentHUD.playerFighter.FStateManager.CurrentStateFrame.ToString();
            stateName.text = parentHUD.playerFighter.FStateManager.GetCurrentStateName();
            velocityMovement.text = parentHUD.playerFighter.FPhysicsManager.forceMovement.ToString("F3");
            var overallForce = parentHUD.playerFighter.FPhysicsManager.GetOverallForce();
            velocityTotal.text = overallForce.ToString("F3");
            speed.text = overallForce.magnitude.ToString("F2");
            simulationFrame.text = ((int)parentHUD.playerFighter.Runner.Simulation.Tick).ToString();
            Vector2 movement = parentHUD.playerFighter.InputManager.GetMovement(0);
            var temp = stickRect.localPosition;
            temp.x = Remap(movement.x, -1.0f, 1.0f, -stickRadius, stickRadius);
            temp.y = Remap(movement.y, -1.0f, 1.0f, -stickRadius, stickRadius);
            stickRect.localPosition = temp;
        }
        
        public static float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax)
        {
            var fromAbs  =  from - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
           
            var normal = fromAbs / fromMaxAbs;
     
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
     
            var to = toAbs + toMin;
           
            return to;
        }
    }
}