using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public class EffectbankContainer : NetworkBehaviour
    {
        public Dictionary<string, int> effectbankMap = new Dictionary<string, int>();
        public List<IEffectbankDefinition> effectbanks = new List<IEffectbankDefinition>();

        public void AddEffectbank(string effectbankName, IEffectbankDefinition effectbank)
        {
            effectbanks.Add(effectbank);
            effectbankMap.Add(effectbankName, effectbanks.Count - 1);
        }

        public IEffectbankDefinition GetEffectbank(string effectbankName)
        {
            return effectbanks[effectbankMap[effectbankName]];
        }

        public BaseEffect CreateEffect(Vector3 position, Quaternion rotation, string effectbankName, string effectName)
        {
            int effectbankIndex = effectbankMap[effectbankName];
            int effectIndex = effectbanks[effectbankIndex].EffectMap[effectName];

            var key = new NetworkObjectPredictionKey { Byte0 = (byte)Runner.Simulation.Tick, Byte1 = (byte)Object.InputAuthority.PlayerId, Byte2 = (byte)effectbankIndex, Byte3 = (byte)effectIndex };
            BaseEffect effect = Runner.Spawn(effectbanks[effectbankIndex].Effects[effectIndex].effect, position, rotation, null, null, key);
            return effect;
        }
    }
}