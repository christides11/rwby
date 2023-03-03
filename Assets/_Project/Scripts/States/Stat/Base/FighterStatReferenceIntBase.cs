using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class FighterStatReferenceIntBase : FighterStatReferenceBase<int>
    {
        public override FighterStatReferenceBase<int> Copy()
        {
            return new FighterStatReferenceIntBase()
            {
                inverse = inverse,
                statReference = statReference,
                value = value
            };
        }
    }
}