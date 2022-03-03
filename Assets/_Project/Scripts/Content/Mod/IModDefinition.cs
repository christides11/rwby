using System;
using System.Collections.Generic;

namespace rwby
{
    public interface IModDefinition
    {
        byte ModSource { get;  }
        uint ModID { get; }
        string ModIdentifier { get; }
        string Description { get; }
        public Dictionary<Type, IContentParser> ContentParsers { get; }
    }
}