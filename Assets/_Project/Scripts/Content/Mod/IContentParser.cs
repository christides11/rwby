using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public abstract class IContentParser
    {
        [SerializeField] public virtual Type parserType { get; }


        public abstract void Initialize();

        public virtual bool ContentExist(string contentIdentfier)
        {
            return false;
        }

        public abstract UniTask<bool> LoadContentDefinitions();

        public abstract UniTask<bool> LoadContentDefinition(string contentIdentifier);

        public virtual IContentDefinition GetContentDefinition(string contentIdentifier)
        {
            return null;
        }

        public virtual List<IContentDefinition> GetContentDefinitions()
        {
            return null;
        }

        public virtual void UnloadContentDefinitions()
        {

        }

        public virtual void UnloadContentDefinition(string contentIdentifier)
        {

        }
    }
}