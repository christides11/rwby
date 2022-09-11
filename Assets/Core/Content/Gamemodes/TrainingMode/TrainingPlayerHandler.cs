using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    [OrderAfter(typeof(ClientManager))]
    [OrderBefore(typeof(FighterInputManager))]
    public class TrainingPlayerHandler : NetworkBehaviour, IFighterCallbacks
    {
        public void FighterHealthChanged(FighterManager fm)
        {
            if(fm.HealthManager.Health <= 0) fm.HealthManager.SetHealth(fm.fighterDefinition.Health);
        }
    }
}