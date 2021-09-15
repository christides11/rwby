using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public abstract class ISoundbank : IContentDefinition
    {
        public override string Name { get; }

        public abstract void GetSound();
    }
}