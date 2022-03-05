using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    public class MovementMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager cm = playerData as FighterManager;

            Vector3 finalSetForce = Vector2.zero;
            Vector3 finalAddForce = Vector2.zero;
            
            Vector2 finalWeights = Vector2.zero;
            float extraWeight = 0;
            
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i) + extraWeight;
                ScriptPlayable<MovementBehaviour> inputPlayable = (ScriptPlayable<MovementBehaviour>)playable.GetInput(i);
                MovementBehaviour input = inputPlayable.GetBehaviour();

                switch (input.forceSetType)
                {
                    case ForceSetType.SET:
                        finalSetForce = (finalSetForce * finalWeights.x) + (input.force * inputWeight);
                        finalWeights.x += inputWeight;
                        break;
                    case ForceSetType.ADD:
                        finalAddForce += input.force * inputWeight;
                        finalWeights.y += inputWeight;
                        break;
                }
            }
            finalWeights.Normalize();
            
            //assign the result to the bound object
            if (finalWeights.x > 0)
            {
                cm.FPhysicsManager.forceMovement = finalSetForce * finalWeights.x;
            }

            cm.FPhysicsManager.forceMovement += finalAddForce * finalWeights.y;
        }
    }
}