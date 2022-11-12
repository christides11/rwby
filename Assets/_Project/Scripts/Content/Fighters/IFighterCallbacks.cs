using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public interface IFighterCallbacks
    {
        void FighterHealthChanged(FighterManager fm, int oldValue);
    }
}