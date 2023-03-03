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
                case StatReferenceType.VARIABLE:
                    return fm.StatManager.GetFighterStats().animationCurveStats[variable];
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override FighterStatReferenceBase<AnimationCurve> Copy()
        {
            return new FighterBaseStatReferenceAnimationCurve()
            {
                inverse = inverse,
                statReference = statReference,
                value = value,
                variable = variable
            };
        }
    }
}