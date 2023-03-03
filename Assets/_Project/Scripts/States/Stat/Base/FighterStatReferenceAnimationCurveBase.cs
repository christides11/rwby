using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class FighterStatReferenceAnimationCurveBase : FighterStatReferenceBase<AnimationCurve>
    {
        public override FighterStatReferenceBase<AnimationCurve> Copy()
        {
            return new FighterStatReferenceAnimationCurveBase()
            {
                inverse = inverse,
                statReference = statReference,
                value = value
            };
        }
    }
}