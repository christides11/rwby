using System;

namespace rwby
{
    [UModContentParser("Projectilebank", "Projectilebanks")]
    [System.Serializable]
    public class UModProjectilebankParser : UModContentParser<IProjectilebankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Projectilebank; }
        }
    }
}