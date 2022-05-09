using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "UModModDefinition", menuName = "Mahou/Content/UMod/ModDefinition")]
    public class UModModDefinition : ScriptableObject, IModDefinition
    {
        public byte ModSource
        {
            get { return modSource; }
        }
        public uint ModID
        {
            get { return modID; }
        }
        public string Description { get { return description; } }
        public string ModGUID { get { return modIdentifier; } }
        public Dictionary<int, IContentParser> ContentParsers { get { return contentParserDictionary; } }

        [field: SerializeField] public ModCompatibilityLevel CompatibilityLevel { get; } = ModCompatibilityLevel.OnlyIfContentSelected;
        [field: SerializeField] public ModVersionStrictness VersionStrictness { get; } = ModVersionStrictness.NeedSameVersion;
        
        [SerializeField] private byte modSource;
        [SerializeField] private uint modID;
        [SerializeField] private string modIdentifier;
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