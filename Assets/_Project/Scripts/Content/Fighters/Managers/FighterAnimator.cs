using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public float currentTime;

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
        [Networked, Capacity(10)] public NetworkLinkedList<FighterAnimationNode> layer0 => default;
    }

    [OrderAfter(typeof(FighterStateManager))]
    public class FighterAnimator : NetworkBehaviour
    {
        public Dictionary<ModObjectReference, IAnimationbankDefinition> bankMap =
            new Dictionary<ModObjectReference, IAnimationbankDefinition>();
        [SerializeField] private AnimancerComponent animancer;
        
        [Networked] private FighterAnimationRoot currentAnimationSet { get; set; }
        //[Networked] public bool tickAccurate { get; set; } = false;

        private ManualMixerState animationMixer = new ManualMixerState();

        private void Awake()
        {
            animationMixer.Speed = 0;
            animationMixer.Key = this;
            animancer.Layers[0].Speed = 0.0f;
            animancer.Layers[0].Play(animationMixer, 0.0f);
        }
        
        public void SyncFromState(ForceSetType syncMode, int layer, AnimationEntry[] wantedAnimations)
        {
            if (syncMode == ForceSetType.SET) ClearAnimationSet(layer);
            for (int i = 0; i < wantedAnimations.Length; i++)
            {
                var temp = currentAnimationSet;
                temp.layer0.Add(new FighterAnimationNode()
                {
                    bank = wantedAnimations[i].animationbankReference,
                    animation = wantedAnimations[i].animation,
                    weight = wantedAnimations[i].startWeight,
                    currentTime = wantedAnimations[i].time
                });
                currentAnimationSet = temp;
            }
            
            SyncAnimancer();
        }

        public void SetAnimationWeight(int layer, int index, float weight)
        {
            var fighterAnimationNode = currentAnimationSet.layer0[index];
            fighterAnimationNode.weight = weight;
            currentAnimationSet.layer0.Set(index, fighterAnimationNode);
            
            animancer.Layers[layer].GetChild(index).Weight = weight;
        }

        public void SetAnimationTime(int layer, int index, float time)
        {
            var fighterAnimationNode = currentAnimationSet.layer0[index];
            fighterAnimationNode.currentTime = time;
            currentAnimationSet.layer0.Set(index, fighterAnimationNode);

            animancer.Layers[layer].GetChild(index).Time = time;
        }

        public override void FixedUpdateNetwork()
        {
            
        }

        AnimationClip[] clips = new AnimationClip[10];
        private bool firstTime = true;
        private void SyncAnimancer()
        {
            if (currentAnimationSet.layer0.Count == 0)
            {
                return;
            }
            
            for (int i = 0; i < 10; i++)
            {
                if (i < currentAnimationSet.layer0.Count)
                {
                    clips[i] = (bankMap[currentAnimationSet.layer0[i].bank]).Animations[currentAnimationSet.layer0[i].animation].clip;
                }
                else
                {
                    clips[i] = null;
                    break;
                }
            }
            
            animationMixer.Initialize(clips);
            for (int i = 0; i < currentAnimationSet.layer0.Count; i++)
            {
                var tempChild = animationMixer.GetChild(i);
                tempChild.Weight = currentAnimationSet.layer0[i].weight;
                tempChild.Time = currentAnimationSet.layer0[i].currentTime;
            }
        }

        public void ClearAnimationSet(int layer)
        {
            var temp = currentAnimationSet;
            temp.layer0.Clear();
            currentAnimationSet = temp;
        }

        public void RegisterBank(ModObjectReference bank)
        {
            if (bankMap.ContainsKey(bank)) return;
            bankMap.Add(bank, ContentManager.singleton.GetContentDefinition<IAnimationbankDefinition>(bank));
        }
    }
}