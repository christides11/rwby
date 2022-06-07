using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class IEffectbankDefinition : IContentDefinition
    {
        public virtual List<EffectbankEffectEntry> Animations { get; }
        public virtual Dictionary<string, int> EffectMap { get; }

        public override UniTask<bool> Load()
        {
            throw new System.NotImplementedException();
        }

        public virtual EffectbankEffectEntry GetEffect(string effect)
        {
            return Animations[EffectMap[effect]];
        }

        public override bool Unload()
        {
            throw new System.NotImplementedException();
        }
    }
}