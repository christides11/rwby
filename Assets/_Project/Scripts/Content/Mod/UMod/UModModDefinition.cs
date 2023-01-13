using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [CreateAssetMenu(fileName = "UModModDefinition", menuName = "rwby/Content/UMod/ModDefinition")]
    public class UModModDefinition : ScriptableObject, IModDefinition
    {
        [System.Serializable]
        public class IdentifierAssetStringRelation
        {
            public string identifier;
            public UModAssetReference asset;
        }
        public string Description { get { return description; } }
        [SerializeField] public uint ModID
        {
            get { return modID; }
        }
        public string ModNamespace
        {
            get => modNamespace;
            set => modNamespace = value;
        }
        public Dictionary<int, IContentParser> ContentParsers { get { return contentParserDictionary; } }
        public ModCompatibilityLevel CompatibilityLevel => compatibilityLevel;
        public ModVersionStrictness VersionStrictness => versionStrictness;
        
        
        [SerializeField] private ModCompatibilityLevel compatibilityLevel = ModCompatibilityLevel.OnlyIfContentSelected;
        [SerializeField] private ModVersionStrictness versionStrictness = ModVersionStrictness.NeedSameVersion;
        [FormerlySerializedAs("realGUID")] [SerializeField] private uint modID;
        [SerializeField] private string modNamespace;
        [TextArea] [SerializeField] private string description;
        [SerializeReference] public List<IContentParser> contentParsers = new List<IContentParser>();
        [NonSerialized] public Dictionary<int, IContentParser> contentParserDictionary = new Dictionary<int, IContentParser>();

        private void OnEnable()
        {
            contentParserDictionary.Clear();
            foreach (IContentParser parser in contentParsers)
            {
                parser.Initialize();
                ContentParsers.Add(parser.parserType, parser);
            }
        }
    }
}