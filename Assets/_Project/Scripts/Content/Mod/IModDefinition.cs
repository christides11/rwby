using System;
using System.Collections.Generic;

namespace rwby
{
    public interface IModDefinition
    {
        uint ModID { get; }
        string ModNamespace { get; set; }
        string Description { get; }
        public Dictionary<int, IContentParser> ContentParsers { get; }
        public ModCompatibilityLevel CompatibilityLevel { get; }
        public ModVersionStrictness VersionStrictness { get; }
    }
}