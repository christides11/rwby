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
        
        [NonSerialized] public Dictionary<string, int> GUIDToInt = new Dictionary<string, int>();
        [NonSerialized] public Dictionary<int, string> IntToGUID = new Dictionary<int, string>();

        public abstract void Initialize();

        public virtual bool ContentExist(string contentIdentfier)
        {
            return false;
        }

        public virtual bool ContentExist(int contentIdentifier)
        {
            return false;
        }

        public abstract UniTask<List<int>> LoadContentDefinitions(LoadedModDefinition modDefinition);
        
        public abstract UniTask<bool> LoadContentDefinition(LoadedModDefinition modDefinition, string contentIdentifier);
        public abstract UniTask<bool> LoadContentDefinition(LoadedModDefinition modDefinition, int index);

        public virtual List<IContentDefinition> GetContentDefinitions()
        {
            return null;
        }
        public virtual IContentDefinition GetContentDefinition(string contentIdentifier)
        {
            return null;
        }
        public virtual IContentDefinition GetContentDefinition(int index)
        {
            return null;
        }

        public virtual void UnloadContentDefinitions()
        {

        }
        public virtual void UnloadContentDefinition(string contentIdentifier)
        {

        }
        public virtual void UnloadContentDefinition(int index)
        {

        }
    }
}