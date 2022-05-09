using System;
using System.Collections.Generic;

namespace rwby
{
    public interface IModDefinition
    {
        string ModGUID { get; }
        string Description { get; }
        public Dictionary<int, IContentParser> ContentParsers { get; }
        public ModCompatibilityLevel CompatibilityLevel { get; }
        public ModVersionStrictness VersionStrictness { get; }
    }
}