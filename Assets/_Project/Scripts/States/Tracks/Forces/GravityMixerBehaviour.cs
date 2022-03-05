using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    public class GravityMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager cm = playerData as FighterManager;

            float finalSetGravity = 0;
            float finalAddGravity = 0;
            
            Vector2 finalWeights = Vector2.zero;
            float extraWeight = 0;
            
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i) + extraWeight;
                ScriptPlayable<GravityBehaviour> inputPlayable = (ScriptPlayable<GravityBehaviour>)playable.GetInput(i);
                GravityBehaviour input = inputPlayable.GetBehaviour();

                switch (input.forceSetType)
                {
                    case ForceSetType.SET:
                        finalSetGravity = (finalSetGravity * finalWeights.x) + (input.force * inputWeight);
                        finalWeights.x += inputWeight;
                        break;
                    case ForceSetType.ADD:
                        finalAddGravity += input.force * inputWeight;
                        finalWeights.y += inputWeight;
                        break;
                }
            }
            finalWeights.Normalize();
            
            //assign the result to the bound object
            if (finalWeights.x > 0)
            {
                cm.FPhysicsManager.forceGravity = finalSetGravity * finalWeights.x;
            }
            cm.FPhysicsManager.forceGravity += finalAddGravity * finalWeights.y;
        }
    }
}