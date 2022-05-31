using System.Collections;
using System.Collections.Generic;
using System.Text;
using rwby;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI.Extensions;

namespace rwby
{
    public class DebugHUDElement : HUDElement
    {
        public TextMeshProUGUI simulationFrame;
        public TextMeshProUGUI position;
        public TextMeshProUGUI stateFrame;
        public TextMeshProUGUI stateFrameTotal;
        public TextMeshProUGUI stateName;
        public TextMeshProUGUI velocityTotal;
        public TextMeshProUGUI velocityMovement;
        public TextMeshProUGUI speed;

        [Header("Input Viewer")]
        [FormerlySerializedAs("stickMaxRange")] public int stickRadius = 49;
        public RectTransform stickRect;
        public UICircle aCircle;
        public UICircle bCircle;
        public UICircle cCircle;
        public UICircle jumpCircle;
        public UICircle blockCircle;
        public UICircle dashCircle;
        public UICircle lockonCircle;
        public UICircle ability1Circle;
        public UICircle ability2Circle;
        public UICircle ability3Circle;
        public UICircle ability4Circle;
        public UICircle extra1Circle;
        public UICircle extra2Circle;
        public UICircle extra3Circle;
        public UICircle extra4Circle;

        public override void InitializeElement(BaseHUD parentHUD)
        {
            
        }

        public override void UpdateElement(BaseHUD parentHUD)
        {
            position.text = parentHUD.playerFighter.myTransform.position.ToString("F1");
            stateFrame.text = parentHUD.playerFighter.FStateManager.CurrentStateFrame.ToString();
            stateFrameTotal.text = parentHUD.playerFighter.FStateManager.GetState().totalFrames.ToString();
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

            UpdateCircle(parentHUD.playerFighter.InputManager.GetButton((int)PlayerInputType.A, 0), aCircle);
            UpdateCircle(parentHUD.playerFighter.InputManager.GetButton((int)PlayerInputType.B, 0), bCircle);
            UpdateCircle(parentHUD.playerFighter.InputManager.GetButton((int)PlayerInputType.C, 0), cCircle);
            UpdateCircle(parentHUD.playerFighter.InputManager.GetButton((int)PlayerInputType.JUMP, 0), jumpCircle);
        }

        private void UpdateCircle(InputButtonData buttonData, UICircle uiCircle)
        {
            uiCircle.color = Color.white;
            if(buttonData.firstPress) uiCircle.color = Color.green;
            if (buttonData.released) uiCircle.color = Color.red;
            uiCircle.SetFill(buttonData.isDown);
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