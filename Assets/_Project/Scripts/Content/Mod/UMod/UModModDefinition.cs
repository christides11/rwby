using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "UModModDefinition", menuName = "rwby/Content/UMod/ModDefinition")]
    public class UModModDefinition : ScriptableObject, IModDefinition
    {
        [System.Serializable]
        public class IdentifierAssetStringRelation
        {
            public ContentGUID identifier;
            public UModAssetReference asset;
        }
        public string Description { get { return description; } }
        public ContentGUID ModGUID { get { return guid; } }
        public string ModID {
            get { return guid.ToString(); }
        }
        public Dictionary<int, IContentParser> ContentParsers { get { return contentParserDictionary; } }
        [field: SerializeField] public ModCompatibilityLevel CompatibilityLevel { get; } = ModCompatibilityLevel.OnlyIfContentSelected;
        [field: SerializeField] public ModVersionStrictness VersionStrictness { get; } = ModVersionStrictness.NeedSameVersion;
        
        [SerializeField] private ContentGUID guid = new ContentGUID(8);
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