using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesModDefinition", menuName = "rwby/Content/Addressables/ModDefinition")]
    public class AddressablesModDefinition : ScriptableObject, IModDefinition
    {
        [System.Serializable]
        public class IdentifierAssetReferenceRelation<T> where T : UnityEngine.Object
        {
            public byte identifier;
            public AssetReferenceT<T> asset;
        }

        public byte ModSource
        {
            get { return modSource; }
        }
        public uint ModID
        {
            get { return modID; }
        }
        public string Description { get { return description; } }
        public string ModStringID { get { return modIdentifier; } }
        public Dictionary<Type, IContentParser> ContentParsers { get { return contentParserDictionary; } }

        [SerializeField] private byte modSource;
        [SerializeField] private uint modID;
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