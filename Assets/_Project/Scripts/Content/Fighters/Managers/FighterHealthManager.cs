using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class FighterHealthManager : HealthManager
    {
        public FighterManager manager;

        public override void ModifyHealth(int amt)
        {
            base.ModifyHealth(amt);
            ((IFighterCallbacks)manager.callbacks).FighterHealthChanged(manager);
        }
    }
}