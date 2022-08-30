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
        [HideInInspector] public Dictionary<ModObjectSetContentReference, int> bankMap = new Dictionary<ModObjectSetContentReference, int>();
        [HideInInspector] public List<IEffectbankDefinition> banks = new List<IEffectbankDefinition>();

        [Networked, Capacity(10)] public FighterEffectsRoot effects { get; set; }
        [Networked, Capacity(10)] public NetworkArray<NetworkBool> autoIncrementEffect => default;
        [Networked] public int effectBufferPos { get; set; } = 0;
        [Networked, Capacity(10)] public NetworkLinkedList<int> currentEffectIndex => default;

        private bool dirty;

        private FighterEffectsRoot currentEffectsRepresentation;

        public BaseEffect[] effectObjects = new BaseEffect[10];
        
        private void Awake()
        {
            dirty = true;
        }

        public void SetEffects(EffectReference[] wantedEffects)
        {
            AddEffects(wantedEffects);
        }

        public void AddEffects(EffectReference[] wantedEffects)
        {
            var temp = effects;
            for (int i = 0; i < wantedEffects.Length; i++)
            {
                temp.effects.Set(effectBufferPos % 10, new FighterEffectNode()
                {
                    bank = bankMap[wantedEffects[i].effectbank]+1,
                    effect = banks[bankMap[wantedEffects[i].effectbank]].EffectMap[wantedEffects[i].effect]+1,
                    frame = 0,
                    parented = wantedEffects[i].parented,
                    pos = wantedEffects[i].offset,
                    rot = wantedEffects[i].rotation,
                    scale = wantedEffects[i].scale
                });
                autoIncrementEffect.Set((effectBufferPos % 10), wantedEffects[i].autoIncrement);

                currentEffectIndex.Add(effectBufferPos);
                effectBufferPos++;
            }
            effects = temp;
            dirty = true;
        }

        public void ClearCurrentEffects(bool removeEffects = false)
        {
            if (removeEffects)
            {
                var temp = effects;
                for (int i = 0; i < currentEffectIndex.Count; i++)
                {
                    temp.effects.Set((currentEffectIndex[i] % 10), new FighterEffectNode());
                }
                effects = temp;
            }
            currentEffectIndex.Clear();
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
                if (effectToModify[i] >= currentEffectIndex.Count) return;
                var temp = effects;
                var t = temp.effects[currentEffectIndex[effectToModify[i]] % 10];
                t.frame += frame;
                temp.effects.Set(currentEffectIndex[effectToModify[i]] % 10, t);
                effects = temp;
            }
            dirty = true;
        }
        
        public void SetEffectRotation(int[] effectToModify, Vector3 rot)
        {
            for (int i = 0; i < effectToModify.Length; i++)
            {
                var temp = effects;
                var t = temp.effects[effectToModify[i]];
                t.rot = rot;
                temp.effects.Set(effectToModify[i], t);
                effects = temp;
            }
            dirty = true;
        }
        
        public void AddEffectRotation(int[] effectToModify, Vector3 rot)
        {
            for (int i = 0; i < effectToModify.Length; i++)
            {
                var temp = effects;
                var t = temp.effects[effectToModify[i]];
                t.rot += rot;
                temp.effects.Set(effectToModify[i], t);
                effects = temp;
            }
            dirty = true;
        }

        public override void FixedUpdateNetwork()
        {
            for (int i = 0; i < 10; i++)
            {
                if (autoIncrementEffect[i])
                {
                    var temp = effects;
                    var t = temp.effects[i];
                    t.frame += 1;
                    temp.effects.Set(i, t);
                    effects = temp;
                    dirty = true;
                }
            }
            
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
                    effectObjects[i].transform.localScale = effects.effects[i].scale;
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
        
        public void RegisterBank(ModObjectSetContentReference bank)
        {
            if (bankMap.ContainsKey(bank)) return;
            banks.Add(ContentManager.singleton.GetContentDefinition<IEffectbankDefinition>(
                ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference(bank.modGUID, (int)ContentType.Effectbank, bank.contentGUID))));
            bankMap.Add(bank, banks.Count-1);
        }
    }
}