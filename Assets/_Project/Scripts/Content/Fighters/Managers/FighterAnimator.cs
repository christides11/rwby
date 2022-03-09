using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Animancer;

namespace rwby
{
    [System.Serializable]
    public enum AnimationMixerType
    {
        NONE,
        MANUAL,
        LINEAR,
        CARTESIAN,
        DIRECTIONAL
    }
    
    public struct FighterAnimationNode : INetworkStruct, IEquatable<FighterAnimationNode>
    {
        public ModObjectReference bank;
        public int animation;
        public float weight;
        public float normalizedTime;

        public override bool Equals(object obj)
        {
            return obj is FighterAnimationNode && this == (FighterAnimationNode)obj;
        }

        public bool Equals(FighterAnimationNode other)
        {
            return bank == other.bank && animation == other.animation;
        }
        
        public static bool operator ==(FighterAnimationNode a, FighterAnimationNode b)
        {
            return a.bank == b.bank && a.animation == b.animation;
        }

        public static bool operator !=(FighterAnimationNode a, FighterAnimationNode b)
        {
            return !(a == b);
        }
    }
    
    public struct FighterAnimationRoot : INetworkStruct
    {
        public float weight;
        [Networked, Capacity(10)] public NetworkLinkedList<FighterAnimationNode> anims => default;
    }

    [OrderAfter(typeof(FighterStateManager))]
    public class FighterAnimator : NetworkBehaviour
    {
        public Dictionary<ModObjectReference, IAnimationbankDefinition> bankMap =
            new Dictionary<ModObjectReference, IAnimationbankDefinition>();
        [SerializeField] private AnimancerComponent animancer;
        
        [Networked] private FighterAnimationRoot currentAnimation { get; set; }
        [Networked] public bool tickAccurate { get; set; } = false;

        private ManualMixerState animationMixer = new ManualMixerState();
        
        private void Awake()
        {
            animationMixer.Speed = 0;
            animationMixer.Key = this;
        }

        AnimationClip[] clips = new AnimationClip[10];
        public void SyncFromState(FighterAnimationRoot wantedAnimations, int layer)
        {
            currentAnimation = wantedAnimations;
            for (int i = 0; i < 10; i++)
            {
                if (i < currentAnimation.anims.Count)
                {
                    if (!bankMap.ContainsKey(currentAnimation.anims[i].bank)) GrabBank(currentAnimation.anims[i].bank);
                    clips[i] = (bankMap[currentAnimation.anims[i].bank]).Animations[currentAnimation.anims[i].animation]
                        .clip;
                }
                else
                {
                    clips[i] = null;
                }
            }
            
            animationMixer.Initialize(clips);
            animationMixer.GetChild(0).Weight = 1.0f;
            for (int a = 0; a < currentAnimation.anims.Count; a++)
            {
                AnimancerState b = animationMixer.GetChild(a);
                b.Weight = currentAnimation.anims[a].weight;
                b.NormalizedTime = currentAnimation.anims[a].normalizedTime;
            }
            animancer.Play(animationMixer);
        }

        private void GrabBank(ModObjectReference bank)
        {
            bankMap.Add(bank, ContentManager.singleton.GetContentDefinition<IAnimationbankDefinition>(bank));
        }
    }
}