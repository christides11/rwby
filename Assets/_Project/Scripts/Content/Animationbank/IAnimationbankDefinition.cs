using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class IAnimationbankDefinition : IContentDefinition
    {
        public virtual List<AnimationbankAnimationEntry> Animations { get; }
        public virtual Dictionary<string, int> AnimationMap { get; }

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