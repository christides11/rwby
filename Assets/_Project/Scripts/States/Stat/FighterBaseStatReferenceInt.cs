using System;

namespace rwby
{
    public class FighterBaseStatReferenceInt : FighterStatReferenceIntBase
    {
        public FighterIntBaseStats variable;

        public override int GetValue(FighterManager fm)
        {
            switch (statReference)
            {
                case StatReferenceType.VALUE:
                    return inverse ? -value : value;
                    break;
                case StatReferenceType.VARIABLE:
                    return inverse ? -fm.StatManager.GetFighterStats().intStats[variable] : fm.StatManager.GetFighterStats().intStats[variable];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return 0;
        }
    }
}