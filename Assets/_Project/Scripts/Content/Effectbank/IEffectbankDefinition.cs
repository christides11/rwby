using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class IEffectbankDefinition : IContentDefinition
    {
        public virtual List<EffectbankEffectEntry> Effects { get; }
        public virtual Dictionary<string, int> EffectMap { get; }
    }
}