using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    public class BepInExModDefinition : ScriptableObject, IModDefinition
    {
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
        public Dictionary<int, IContentParser> ContentParsers
        {
            get { return null; }
        }
        [field: SerializeField] public ModCompatibilityLevel CompatibilityLevel { get; } = ModCompatibilityLevel.OnlyIfContentSelected;
        [field: SerializeField] public ModVersionStrictness VersionStrictness { get; } = ModVersionStrictness.NeedSameVersion;
        
        [FormerlySerializedAs("realGUID")] [SerializeField] private uint modID;
        [SerializeField] private string modNamespace;
        [TextArea] [SerializeField] private string description;

        private void OnEnable()
        {
            
        }
    }
}