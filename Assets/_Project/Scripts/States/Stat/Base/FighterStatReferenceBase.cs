using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class FighterStatReferenceBase<T>
    {
        protected bool StatReferenceIsValue () { return statReference == StatReferenceType.VALUE; }
        
        public bool inverse;
        public StatReferenceType statReference;
        [ShowIf("StatReferenceIsValue"), AllowNesting]
        public T value;

        public virtual T GetValue(FighterManager fm)
        {
            return value;
        }
    }
}