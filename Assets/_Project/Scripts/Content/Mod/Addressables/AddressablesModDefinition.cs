using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesModDefinition", menuName = "rwby/Content/Addressables/ModDefinition")]
    public class AddressablesModDefinition : ScriptableObject, IModDefinition
    {
        [System.Serializable]
        public class IdentifierAssetReferenceRelation<T> where T : UnityEngine.Object
        {
            public ContentGUID identifier;
            public AssetReferenceT<T> asset;
        }
        public string Description { get { return description; } }
        public ContentGUID ModGUID { get { return guid; } }
        public string ModID {
            get { return guid.ToString(); }
        }
        public Dictionary<int, IContentParser> ContentParsers { get { return contentParserDictionary; } }
        public ModCompatibilityLevel CompatibilityLevel
        {
            get { return compatibilityLevel; }
        }
        public ModVersionStrictness VersionStrictness
        {
            get { return versionStrictness; }
        }

        [SerializeField] private ModCompatibilityLevel compatibilityLevel = ModCompatibilityLevel.OnlyIfContentSelected;
        [SerializeField] private ModVersionStrictness versionStrictness = ModVersionStrictness.NeedSameVersion;
        [FormerlySerializedAs("guida")] [SerializeField] private ContentGUID guid = new ContentGUID(8);
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