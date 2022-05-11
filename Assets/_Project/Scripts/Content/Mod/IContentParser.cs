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
        [SerializeField, HideInInspector] private string name = "Generic Parser";
        [SerializeField] public virtual int parserType { get; }

        public abstract void Initialize();

        public virtual bool ContentExist(ContentGUID contentIdentfier)
        {
            return false;
        }

        public abstract UniTask<List<ContentGUID>> LoadContentDefinitions();

        public abstract UniTask<bool> LoadContentDefinition(ContentGUID contentIdentifier);

        public virtual IContentDefinition GetContentDefinition(ContentGUID contentIdentifier)
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

        public virtual void UnloadContentDefinition(ContentGUID contentIdentifier)
        {

        }
    }
}