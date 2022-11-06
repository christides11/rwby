using System;

namespace rwby
{
    [AddressablesContentParser("Song", "Songs")]
    public class AddressablesSongParser : AddressablesContentParser<ISongDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Song; }
        }
    }
}