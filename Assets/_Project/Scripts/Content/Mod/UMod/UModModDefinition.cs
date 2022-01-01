using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "UModModDefinition", menuName = "Mahou/Content/UMod/ModDefinition")]
    public class UModModDefinition : ScriptableObject, IModDefinition
    {
        public string Description { get { return description; } }
        public string ModIdentifier { get { return modIdentifier; } }
        public Dictionary<Type, IContentParser> ContentParsers { get { return contentParserDictionary; } }

        [SerializeField] private string modIdentifier;
        [TextArea] [SerializeField] private string description;
        [SerializeReference] public List<IContentParser> contentParsers = new List<IContentParser>();
        [NonSerialized] public Dictionary<Type, IContentParser> contentParserDictionary = new Dictionary<Type, IContentParser>();

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