using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class IEffectbankDefinition : IContentDefinition
    {
        public virtual List<EffectbankEffectEntry> Effects { get; }
        public virtual Dictionary<string, int> EffectMap { get; }

        public override UniTask<bool> Load()
        {
            throw new System.NotImplementedException();
        }

        public override bool Unload()
        {
            throw new System.NotImplementedException();
        }
    }
}