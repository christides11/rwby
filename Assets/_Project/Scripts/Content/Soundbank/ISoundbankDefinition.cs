using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public abstract class ISoundbankDefinition : IContentDefinition
    {
        public virtual List<SoundbankSoundEntry> Sounds { get; }
        public virtual Dictionary<string, int> SoundMap { get; }

        public override UniTask<bool> Load()
        {
            throw new System.NotImplementedException();
        }

        public virtual SoundbankSoundEntry GetEffect(string effect)
        {
            return Sounds[SoundMap[effect]];
        }
        
        public override bool Unload()
        {
            throw new System.NotImplementedException();
        }
    }
}