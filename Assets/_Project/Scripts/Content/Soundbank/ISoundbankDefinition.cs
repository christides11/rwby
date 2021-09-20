using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public abstract class ISoundbankDefinition : IContentDefinition
    {
        public override string Name { get; }
        public virtual List<SoundbankSoundEntry> Sounds { get; }
        public virtual Dictionary<string, int> SoundMap { get; }
    }
}