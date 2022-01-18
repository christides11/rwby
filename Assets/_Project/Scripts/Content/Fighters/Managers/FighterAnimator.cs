using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Animancer;

namespace rwby
{
    public struct FighterAnimDefinition : INetworkStruct
    {
        public int animationbank;
        public int animation;
    }

    [OrderAfter(typeof(FighterStateManager))]
    public class FighterAnimator : NetworkBehaviour
    {
        [SerializeField] private AnimationbankContainer container;
        [SerializeField] private AnimancerComponent animancer;

        [Networked] public FighterAnimDefinition currentAnimation { get; set; }
        [Networked] public int currentFrame { get; set; }
        [Networked] public NetworkBool rollbackTickAccurate { get; set; }

        public FighterAnimDefinition localAnimation;

        public void Play(string animationbank, string animation)
        {
            IAnimationbankDefinition bank = container.GetAnimationbank(animationbank);

            currentAnimation = new FighterAnimDefinition()
            {
                animationbank = container.animationbankMap[animationbank] + 1,
                animation = bank.AnimationMap[animation] + 1
            };
            currentFrame = 0;
        }

        public void Stop()
        {
            currentAnimation = new FighterAnimDefinition()
            {
                animationbank = 0,
                animation = 0
            };
            currentFrame = 0;
        }

        public override void FixedUpdateNetwork()
        {
            // TODO: Have animations run even during rollbacks?
            // Maybe have a toggle that can be turned on when this is actually needed.
            if (Runner.IsForward == false) return;

            if(localAnimation.animationbank != currentAnimation.animationbank
                || localAnimation.animation != currentAnimation.animation)
            {
                localAnimation = currentAnimation;

                // No animation.
                if (currentAnimation.animationbank == 0 || currentAnimation.animation == 0)
                {
                    animancer.Stop();
                }
                else
                {
                    IAnimationbankDefinition bank = container.animationbanks[currentAnimation.animationbank - 1];
                    var state = animancer.Play(bank.Animations[currentAnimation.animation - 1].clip);
                    state.Speed = 0;
                    state.Time = currentFrame * Runner.DeltaTime;
                }
            }

            if (currentAnimation.animationbank == 0 || currentAnimation.animation == 0) return;
            animancer.States.Current.Time = currentFrame * Runner.DeltaTime;
        }
    }
}