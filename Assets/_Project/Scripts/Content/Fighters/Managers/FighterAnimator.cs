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
    [OrderBefore(typeof(CombatPairFinder))]
    [OrderAfter(typeof(FighterStateManager))]
    public class FighterAnimator : NetworkBehaviour
    {
        public enum AnimationSetDirtyReason
        {
            NOT_DIRTY,
            ANIMATION_NODE_DIRTY,
            ANIMATION_SET_DIRTY
        }
        [HideInInspector] public Dictionary<ModObjectGUIDReference, int> bankMap = new Dictionary<ModObjectGUIDReference, int>();
        [HideInInspector] public List<IAnimationbankDefinition> banks = new List<IAnimationbankDefinition>();
        [SerializeField] private AnimancerComponent animancer;
        
        [Networked] private FighterAnimationRoot animationSet { get; set; }
        private bool animDirty = true;

        private FighterAnimationRoot currentAnimancerRepresentation;

        public void Awake()
        {
            animancer.Layers[0].Weight = 1.0f;
        }

        public void SetAnimationSet(int layer, AnimationReference[] wantedAnimations, float fadeTime = 0.0f)
        {
            ClearAnimationSet(layer);
            AddAnimationToSet(layer, wantedAnimations);
        }

        public void AddAnimationToSet(int layer, AnimationReference[] wantedAnimations)
        {
            Profiler.BeginSample("ADD ANIMATION TO SET", gameObject);
            for (int i = 0; i < wantedAnimations.Length; i++)
            {
                var temp = animationSet;
                temp.layer0.Add(new FighterAnimationNode()
                {
                    bank = bankMap[wantedAnimations[i].animationbank],
                    animation = banks[bankMap[wantedAnimations[i].animationbank]].AnimationMap[wantedAnimations[i].animation],
                    frame = 0,
                    weight = 1.0f
                });
                animationSet = temp;
            }

            animDirty = true;
            Profiler.EndSample();
        }

        public void SetAnimationWeight(int layer, int[] animations, float weight)
        {
            var temp = animationSet;
            for (int i = 0; i < animations.Length; i++)
            {
                var temp2 = temp.layer0[animations[i]];
                temp2.weight = weight;
                temp.layer0.Set(animations[i], temp2);
            }
            animationSet = temp;
            animDirty = true;
        }
        
        public void AddAnimationWeight(int layer, int[] animations, float weight)
        {
            var temp = animationSet;
            for (int i = 0; i < animations.Length; i++)
            {
                var temp2 = temp.layer0[animations[i]];
                temp2.weight += weight;
                temp.layer0.Set(animations[i], temp2);
            }
            animationSet = temp;
            animDirty = true;
        }

        public void SetAnimationTime(int layer, int[] animations, int frame)
        {
            var temp = animationSet;
            for (int i = 0; i < animations.Length; i++)
            {
                var temp2 = temp.layer0[animations[i]];
                temp2.frame = frame;
                temp.layer0.Set(animations[i], temp2);
            }
            animationSet = temp;
            animDirty = true;
        }
        
        public void AddAnimationTime(int layer, int[] animations, int frame)
        {
            var temp = animationSet;
            for (int i = 0; i < animations.Length; i++)
            {
                var temp2 = temp.layer0[animations[i]];
                temp2.frame += frame;
                temp.layer0.Set(animations[i], temp2);
            }
            animationSet = temp;
            animDirty = true;
        }
        
        public override void Render()
        {
            base.Render();
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsResimulation && Runner.IsFirstTick && currentAnimancerRepresentation != animationSet)
            {
                SyncAnimancer();
                animDirty = false;
            }

            if (animDirty)
            {
                SyncAnimancer();
                animDirty = false;
            }
        }
        
        private void SyncAnimancer()
        {
            Profiler.BeginSample("SYNCING ANIMANCER", gameObject);
            
            Profiler.BeginSample("UPDATING REPRESENTATION", gameObject);
            currentAnimancerRepresentation = animationSet;
            ClearWeights(0);
            if (animationSet.layer0.Count == 0)
            {
                Profiler.EndSample();
                Profiler.EndSample();
                return;
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("UPDATING CLIPS", gameObject);
            for (int i = 0; i < animationSet.layer0.Count; i++)
            {
                var c = banks[animationSet.layer0[i].bank].Animations[animationSet.layer0[i].animation].clip;
                var cState = animancer.States.GetOrCreate(c);
                cState.Weight = animationSet.layer0[i].weight;
                cState.Time = animationSet.layer0[i].frame * Runner.DeltaTime;
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("APPLY CHANGES", gameObject);
            animancer.Evaluate();
            Profiler.EndSample();
            
            Profiler.EndSample();
        }

        public AnimationClip GetClip(AnimationReference animation)
        {
            return banks[bankMap[animation.animationbank]].GetAnimation(animation.animation).clip;
        }

        public void ClearWeights(int layer)
        {
            for (int i = 0; i < animancer.Layers[0].ChildCount; i++)
            {
                animancer.Layers[0].GetChild(i).Weight = 0;
            }
        }
        
        public void ClearAnimationSet(int layer)
        {
            var temp = animationSet;
            temp.layer0.Clear();
            animationSet = temp;
        }

        public void RegisterBank(ModObjectGUIDReference bank)
        {
            if (bankMap.ContainsKey(bank)) return;
            banks.Add(ContentManager.singleton.GetContentDefinition<IAnimationbankDefinition>(bank));
            bankMap.Add(bank, banks.Count-1);

            for (int i = 0; i < banks[^1].Animations.Count; i++)
            {
                animancer.Layers[0].CreateState(banks[^1].Animations[i].clip);
            }
        }
    }
}