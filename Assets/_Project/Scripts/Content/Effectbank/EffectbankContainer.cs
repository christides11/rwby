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
    }
}