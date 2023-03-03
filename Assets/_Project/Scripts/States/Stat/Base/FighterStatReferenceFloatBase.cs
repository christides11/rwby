namespace rwby
{
    public class FighterStatReferenceFloatBase : FighterStatReferenceBase<float>
    {
        public override FighterStatReferenceBase<float> Copy()
        {
            return new FighterStatReferenceFloatBase()
            {
                inverse = inverse,
                statReference = statReference,
                value = value
            };
        }
    }
}