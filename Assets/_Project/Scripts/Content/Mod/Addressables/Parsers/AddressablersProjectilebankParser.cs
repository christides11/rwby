namespace rwby
{
    [AddressablesContentParser("Projectilebank", "Projectilebank")]
    public class AddressablesProjectilebankParser : AddressablesContentParser<IProjectilebankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Projectilebank; }
        }
    }
}