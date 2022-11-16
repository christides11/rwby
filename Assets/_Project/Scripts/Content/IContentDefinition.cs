using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public abstract class IContentDefinition : ScriptableObject
    {
        public virtual int Identifier { get; set; }
        public virtual string Name { get; }
        public virtual string Description { get; }
        public virtual List<string> Tags => tags;

        [SerializeField] private List<string> tags;
        
        public abstract UniTask<bool> Load();
        public abstract bool Unload();
    }
}