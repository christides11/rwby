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
        [Networked, Capacity(10)] public NetworkArray<NetworkBool> isCurrentEffect => default;
        [Networked, Capacity(10)] public NetworkArray<NetworkBool> effectPaused => default;
        [Networked] public int effectBufferPos { get; set; } = 0;
        [Networked] public NetworkRNG rng { get; set; } = new NetworkRNG(0);

        private bool dirty;
        private FighterEffectsRoot currentEffectsRepresentation;
        public BaseEffect[] effectObjects = new BaseEffect[10];
        public FighterManager manager;
        
        private void Awake()
        {
            dirty = true;
        }
        
        public override void Spawned()
        {
            base.Spawned();
            if(Object.HasStateAuthority) rng = new NetworkRNG(327);
        }

        public void SetEffects(EffectReference[] wantedEffects)
        {
            AddEffects(wantedEffects);
        }

        public void AddEffects(EffectReference[] wantedEffects, Vector3 posBase = default, bool addToEffectSet = true)
        {
            var temp = effects;
            for (int i = 0; i < wantedEffects.Length; i++)
            {
                temp.effects.Set(effectBufferPos % effects.effects.Length, new FighterEffectNode()
                {
                    bank = bankMap[wantedEffects[i].effectbank]+1,
                    effect = banks[bankMap[wantedEffects[i].effectbank]].EffectMap[wantedEffects[i].effect]+1,
                    frame = Runner.Tick,
                    parent = wantedEffects[i].parent == null ? 0 : wantedEffects[i].parent.GetBone(),
                    pos = posBase + wantedEffects[i].offset,
                    rot = wantedEffects[i].rotation,
                    scale = wantedEffects[i].scale
                });
                
                if (addToEffectSet) isCurrentEffect.Set((effectBufferPos % effects.effects.Length), true);
                effectBufferPos++;
            }
            effects = temp;
            dirty = true;
        }

        public void ClearCurrentEffects(bool removeEffects = false, bool autoIncrementEffects = true)
        {
            ResumeCurrentEffects();
            var temp = effects;
            for (int i = 0; i < isCurrentEffect.Length; i++)
            {
                if (!isCurrentEffect[i]) continue;
                if(removeEffects) temp.effects.Set(i, new FighterEffectNode());
                isCurrentEffect.Set(i, false);
            }
            effects = temp;
            
            dirty = true;
        }

        public void SetEffectTime(int[] effectToModify, int frame)
        {
        }
        
        public void AddEffectTime(int[] effectToModify, int frame)
        {
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

        public void PauseCurrentEffects()
        {
            for (int i = 0; i < effectPaused.Length; i++)
            {
                if(isCurrentEffect[i]) PauseEffect(i);
            }
        }
        
        public void ResumeCurrentEffects()
        {
            for (int i = 0; i < effectPaused.Length; i++)
            {
                ResumeEffect(i);
            }
        }

        public void PauseEffect(int index)
        {
            effectPaused.Set(index, true);
            dirty = true;
        }

        public void ResumeEffect(int index)
        {
            effectPaused.Set(index, false);
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

            for (int i = 0; i < effectPaused.Length; i++)
            {
                if (!effectPaused[i]) continue;
                var temp = effects;
                var t = temp.effects[i];
                t.frame += 1;
                temp.effects.Set(i, t);
                effects = temp;
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
                    var e = GetEffect(effects.effects[i]);
                    if (effectObjects[i] && (effectObjects[i].bank != effects.effects[i].bank ||
                                             effectObjects[i].effect != effects.effects[i].effect)
                        || effectObjects[i] == null)
                    {
                        if(effectObjects[i]) Destroy(effectObjects[i].gameObject);
                        effectObjects[i] = GameObject.Instantiate(e, transform, false);
                        effectObjects[i].bank = effects.effects[i].bank;
                        effectObjects[i].effect = effects.effects[i].effect;
                        effectObjects[i].StopEffect(ParticleSystemStopBehavior.StopEmittingAndClear);
                        effectObjects[i].SetRandomSeed((uint)UnityEngine.Random.Range(0, 900));
                    }
                    effectObjects[i].transform.SetParent(effects.effects[i].parent == 0 ? null : 
                        manager.boneRefs[effects.effects[i].parent-1], false);
                    effectObjects[i].transform.localPosition = effects.effects[i].pos;
                    effectObjects[i].transform.localRotation = Quaternion.Euler(effects.effects[i].rot);
                    effectObjects[i].transform.localScale = effects.effects[i].scale;
                }

                if (effectPaused[i])
                {
                    effectObjects[i].PauseEffect();
                    continue;
                }
                
                effectObjects[i].SetFrame((float)(Runner.Tick - effects.effects[i].frame - 1) * Runner.DeltaTime);
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