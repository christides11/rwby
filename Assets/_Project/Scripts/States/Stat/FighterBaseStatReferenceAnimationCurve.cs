using System;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    public class FighterBaseStatReferenceAnimationCurve : FighterStatReferenceAnimationCurveBase
    {
        [HideIf("StatReferenceIsValue"), AllowNesting]
        public FighterAnimationCurveBaseStats variable;
        
        public override AnimationCurve GetValue(FighterManager fm)
        {
            switch (statReference)
            {
                case StatReferenceType.VALUE:
                    return value;
                    break;
                case StatReferenceType.VARIABLE:
                    return fm.StatManager.GetFighterStats().animationCurveStats[variable];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new AnimationCurve();
        }
    }
}