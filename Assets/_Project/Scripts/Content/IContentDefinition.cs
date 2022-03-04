using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public abstract class IContentDefinition : ScriptableObject
    {
        public virtual byte Identifier { get; set; } = 0;
        public virtual string NameIdentifier
        {
            get => nameIdentifier;
            set => nameIdentifier = value;
        }
        public virtual string Name { get; }
        public virtual string Description { get; }

        public string nameIdentifier;

        public abstract UniTask<bool> Load();
        public abstract bool Unload();
    }
}