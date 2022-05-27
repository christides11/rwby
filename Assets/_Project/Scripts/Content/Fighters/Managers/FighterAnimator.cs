using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Animancer;
using UnityEngine.Profiling;

namespace rwby
{
    [OrderAfter(typeof(FighterStateManager))]
    public class FighterAnimator : NetworkBehaviour
    {
        [HideInInspector] public Dictionary<ModObjectGUIDReference, int> bankMap = new Dictionary<ModObjectGUIDReference, int>();
        [HideInInspector] public List<IAnimationbankDefinition> banks = new List<IAnimationbankDefinition>();
        [SerializeField] private AnimancerComponent animancer;
        
        [Networked] private FighterAnimationRoot currentAnimationSet { get; set; }
        // For interpolation.
        //[Networked] private FighterAnimationRoot previousAnimationSet { get; set; }

        private FighterAnimationRoot currentAnimancerRepresentation;

        private ManualMixerState animationMixer = new ManualMixerState();

        private void Awake()
        {
            animationMixer.Speed = 0;
            animationMixer.Key = this;
            animancer.Layers[0].Speed = 0.0f;
            animancer.Layers[0].Play(animationMixer, 0.0f);
        }

        public void SetAnimationSet(int layer, AnimationReference[] wantedAnimations, float fadeTime = 0.0f)
        {
            ClearAnimationSet(layer);
            AddAnimationToSet(layer, wantedAnimations);
            //SyncAnimancer();
        }

        public void AddAnimationToSet(int layer, AnimationReference[] wantedAnimations)
        {
            Profiler.BeginSample("ADD ANIMATION TO SET", gameObject);
            for (int i = 0; i < wantedAnimations.Length; i++)
            {
                var temp = currentAnimationSet;
                temp.layer0.Add(new FighterAnimationNode()
                {
                    bank = bankMap[wantedAnimations[i].animationbank],
                    animation = banks[bankMap[wantedAnimations[i].animationbank]].AnimationMap[wantedAnimations[i].animation],
                    frame = 0,
                    weight = 1.0f
                });
                currentAnimationSet = temp;
            }
            Profiler.EndSample();
            //SyncAnimancer();
        }

        public void SetAnimationWeight(int layer, int index, float weight)
        {
            var fighterAnimationNode = currentAnimationSet.layer0[index];
            fighterAnimationNode.weight = weight;
            currentAnimationSet.layer0.Set(index, fighterAnimationNode);
            animancer.Layers[layer].GetChild(index).Weight = weight;
        }

        public void SetAnimationTime(int layer, int index, int frame)
        {
            var fighterAnimationNode = currentAnimationSet.layer0[index];
            fighterAnimationNode.frame = frame;
            currentAnimationSet.layer0.Set(index, fighterAnimationNode);

            animancer.Layers[layer].GetChild(index).Time = frame * Runner.Simulation.DeltaTime;
        }

        public void AddAnimationTime(int layer, int index, int frame)
        {
            var fighterAnimationNode = currentAnimationSet.layer0[index];
            fighterAnimationNode.frame += frame;
            currentAnimationSet.layer0.Set(index, fighterAnimationNode);

            animancer.Layers[layer].GetChild(index).Time += frame * Runner.Simulation.DeltaTime;
        }

        public override void Render()
        {
            base.Render();
        }

        public override void FixedUpdateNetwork()
        {
            /*
            if (Runner.IsResimulation && Runner.IsFirstTick && currentAnimancerRepresentation != currentAnimationSet)
            {
                SyncAnimancer();
            }*/
        }

        AnimationClip[] clips = new AnimationClip[10];
        private void SyncAnimancer()
        {
            /*
            currentAnimancerRepresentation = currentAnimationSet;
            if (currentAnimationSet.layer0.Count == 0)
            {
                return;
            }
            
            // TODO: Fix animator.
            for (int i = 0; i < 10; i++)
            {
                if (i < currentAnimationSet.layer0.Count)
                {
                    //clips[i] = (bankMap[currentAnimationSet.layer0[i].bank]).Animations[currentAnimationSet.layer0[i].animation].clip;
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
            }*/
        }

        public void ClearAnimationSet(int layer)
        {
            var temp = currentAnimationSet;
            temp.layer0.Clear();
            currentAnimationSet = temp;
        }

        public void RegisterBank(ModObjectGUIDReference bank)
        {
            if (bankMap.ContainsKey(bank)) return;
            banks.Add(ContentManager.singleton.GetContentDefinition<IAnimationbankDefinition>(bank));
            bankMap.Add(bank, banks.Count-1);
        }
    }
}