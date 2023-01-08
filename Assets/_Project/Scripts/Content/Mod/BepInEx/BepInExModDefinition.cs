using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class BepInExModDefinition : ScriptableObject, IModDefinition
    {
        public string Description { get { return description; } }
        public ContentGUID ModGUID { get { return guid; } }
        public string ModID {
            get { return guid.ToString(); }
        }
        public Dictionary<int, IContentParser> ContentParsers
        {
            get { return null; }
        }
        [field: SerializeField] public ModCompatibilityLevel CompatibilityLevel { get; } = ModCompatibilityLevel.OnlyIfContentSelected;
        [field: SerializeField] public ModVersionStrictness VersionStrictness { get; } = ModVersionStrictness.NeedSameVersion;
        
        [SerializeField] private ContentGUID guid = new ContentGUID(8);
        [TextArea] [SerializeField] private string description;

        private void OnEnable()
        {
            
        }
    }
}