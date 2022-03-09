using System;
using NaughtyAttributes;

namespace rwby
{
    public class FighterBaseStatReferenceFloat : FighterStatReferenceFloatBase
    {
        [HideIf("StatReferenceIsValue"), AllowNesting]
        public FighterFloatBaseStats variable;
        
        public override float GetValue(FighterManager fm)
        {
            switch (statReference)
            {
                case StatReferenceType.VALUE:
                    return inverse ? -value : value;
                case StatReferenceType.VARIABLE:
                    return inverse ? -fm.StatManager.GetFighterStats().floatStats[variable] : fm.StatManager.GetFighterStats().floatStats[variable];
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}