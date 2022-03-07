using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    public class RotationMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager fm = playerData as FighterManager;
            
            Vector3 finalSetRotation = Vector2.zero;
            Vector3 finalAddRotation = Vector2.zero;
            
            Vector2 finalWeights = Vector2.zero;
            float extraWeight = 0;
            
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i) + extraWeight;
                ScriptPlayable<RotationBehaviour> inputPlayable = (ScriptPlayable<RotationBehaviour>)playable.GetInput(i);
                RotationBehaviour input = inputPlayable.GetBehaviour();

                switch (input.rotationSetType)
                {
                    case ForceSetType.SET:
                        finalSetRotation = (finalSetRotation * finalWeights.x) + (input.euler * inputWeight);
                        finalWeights.x += inputWeight;
                        break;
                    case ForceSetType.ADD:
                        finalAddRotation += input.euler * inputWeight;
                        finalWeights.y += inputWeight;
                        break;
                }

                input.euler = Vector3.zero;
            }
            finalWeights.Normalize();

            Vector3 rotDirection = Vector3.zero;
            // assign the result to the bound object
            if (finalWeights.x > 0)
            {
                rotDirection = finalSetRotation * finalWeights.x;
            }
            else
            {
                rotDirection = fm.transform.forward;
            }

            rotDirection += finalAddRotation * finalWeights.y;
            if (rotDirection == Vector3.zero) return;
            fm.SetVisualRotation( rotDirection.normalized);
        }
    }
}