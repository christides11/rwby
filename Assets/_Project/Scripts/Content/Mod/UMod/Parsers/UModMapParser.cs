using System;

namespace rwby
{
    [UModContentParser("Map", "Maps")]
    [System.Serializable]
    public class UModMapParser : UModContentParser<IMapDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Map; }
        }
    }
}