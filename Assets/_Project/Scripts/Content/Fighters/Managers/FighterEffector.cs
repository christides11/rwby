using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    [OrderAfter(typeof(FighterStateManager))]
    public class FighterEffector : NetworkBehaviour
    {
        [HideInInspector] public Dictionary<ModGUIDContentReference, int> bankMap = new Dictionary<ModGUIDContentReference, int>();
        [HideInInspector] public List<IEffectbankDefinition> banks = new List<IEffectbankDefinition>();

        [Networked, Capacity(10)] public FighterEffectsRoot effects { get; set; }
        private bool dirty;

        private FighterEffectsRoot currentEffectsRepresentation;

        public BaseEffect[] effectObjects = new BaseEffect[10];

        private void Awake()
        {
            dirty = true;
        }

        public void SetEffects(EffectReference[] wantedEffects)
        {
            ClearEffects();
            AddEffects(wantedEffects);
        }

        public void AddEffects(EffectReference[] wantedEffects)
        {
            for (int i = 0; i < wantedEffects.Length; i++)
            {
                var temp = effects;
                temp.effects.Set(i, new FighterEffectNode()
                {
                    bank = bankMap[wantedEffects[i].effectbank]+1,
                    effect = banks[bankMap[wantedEffects[i].effectbank]].EffectMap[wantedEffects[i].effect]+1,
                    frame = 0,
                    parented = wantedEffects[i].parented,
                    pos = wantedEffects[i].offset,
                    rot = wantedEffects[i].rotation
                });
                effects = temp;
            }
            dirty = true;
        }

        public void SetEffectTime(int[] effectToModify, int frame)
        {
            for (int i = 0; i < effectToModify.Length; i++)
            {
                var temp = effects;
                var t = temp.effects[effectToModify[i]];
                t.frame = frame;
                temp.effects.Set(effectToModify[i], t);
                effects = temp;
            }
            dirty = true;
        }
        
        public void AddEffectTime(int[] effectToModify, int frame)
        {
            for (int i = 0; i < effectToModify.Length; i++)
            {
                var temp = effects;
                var t = temp.effects[effectToModify[i]];
                t.frame += frame;
                temp.effects.Set(effectToModify[i], t);
                effects = temp;
            }
            dirty = true;
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsResimulation)
            {
                if (Runner.IsLastTick && !AreEffectsUpToDate())
                {
                    SyncEffects();
                }
                return;
            }

            if (dirty)
            {
                SyncEffects();
                dirty = false;
            }
            
        }

        private void SyncEffects()
        {
            for (int i = 0; i < effectObjects.Length; i++)
            {
                if (effects.effects[i].bank == 0)
                {
                    if(effectObjects[i]) Destroy(effectObjects[i].gameObject);
                    continue;
                }
                
                if (currentEffectsRepresentation.effects[i] != effects.effects[i])
                {
                    if(effectObjects[i]) Destroy(effectObjects[i].gameObject);
                    var e = GetEffect(effects.effects[i]);
                    effectObjects[i] = effects.effects[i].parented ?
                        GameObject.Instantiate(e, transform, false)
                        : GameObject.Instantiate(e, effects.effects[i].pos, Quaternion.identity);
                    effectObjects[i].transform.localPosition = effects.effects[i].pos;
                    effectObjects[i].transform.localRotation = Quaternion.Euler(effects.effects[i].rot);
                }
                
                effectObjects[i].SetFrame(effects.effects[i].frame * Runner.DeltaTime);
            }
            
            currentEffectsRepresentation = effects;
        }

        private void ClearEffects()
        {
            var temp = effects;
            for (int i = 0; i < temp.effects.Length; i++)
            {
                temp.effects.Set(i, new FighterEffectNode());
            }
            effects = temp;
        }

        private bool AreEffectsUpToDate()
        {
            for (int i = 0; i < effects.effects.Length; i++)
            {
                if(currentEffectsRepresentation.effects[i] != effects.effects[i]) return false;
            }
            return true;
        }
        
        public BaseEffect GetEffect(FighterEffectNode effectNode)
        {
            return banks[effectNode.bank-1].Animations[effectNode.effect-1].effect;
        }
        
        public BaseEffect GetEffect(EffectReference animation)
        {
            return banks[bankMap[animation.effectbank]].GetEffect(animation.effect).effect;
        }
        
        public void RegisterBank(ModGUIDContentReference bank)
        {
            if (bankMap.ContainsKey(bank)) return;
            banks.Add(ContentManager.singleton.GetContentDefinition<IEffectbankDefinition>(bank));
            bankMap.Add(bank, banks.Count-1);
        }
    }
}