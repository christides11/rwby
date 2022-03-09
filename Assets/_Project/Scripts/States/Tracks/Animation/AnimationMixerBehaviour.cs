using UnityEngine.Playables;

namespace rwby
{
    public class AnimationMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = playerData as FighterManager;
            FighterAnimator animator = manager.fighterAnimator;

            FighterAnimationRoot far = new FighterAnimationRoot(){ weight = 1.0f };

            int inputCount = playable.GetInputCount();
            float inputWeight;
            ScriptPlayable<AnimationBehaviour> inputPlayable;
            AnimationBehaviour input;
            for (int i = 0; i < inputCount; i++)
            {
                inputWeight = playable.GetInputWeight(i);
                inputPlayable = (ScriptPlayable<AnimationBehaviour>)playable.GetInput(i);
                input = inputPlayable.GetBehaviour();

                for (int j = 0; j < input.animations.Length; j++)
                {
                    
                    var g = new FighterAnimationNode()
                    {
                        animation = input.animations[j].animation, bank = input.animations[j].animationbankReference, weight = input.animations[j].weight, normalizedTime = input.animations[j].normalizedTime
                    };
                    if (far.anims.Contains(g)) continue;
                    far.anims.Add(g);
                }
            }
            
            animator.SyncFromState(far, 0);
        }
    }
}