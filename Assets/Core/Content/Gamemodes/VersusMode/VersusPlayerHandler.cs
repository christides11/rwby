using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby.core.versus
{
    [OrderAfter(typeof(ClientManager))]
    [OrderBefore(typeof(FighterInputManager))]
    public class VersusPlayerHandler : NetworkBehaviour, IFighterCallbacks
    {
        public GamemodeVersus gamemode;
        
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
        }

        public void FighterHealthChanged(FighterManager fm)
        {
            if (fm.HealthManager.Health <= 0)
            {
                fm.HandleDeath();
                if (Object.HasStateAuthority)
                {
                    var respawnPoint = gamemode.GetRespawnPosition(fm.FCombatManager.Team);
                    fm.FPhysicsManager.SetPosition(respawnPoint.transform.position, true);
                    fm.FPhysicsManager.SetRotation(respawnPoint.transform.eulerAngles, true);
                    fm.HandleRespawn();
                }
                fm.HealthManager.SetHealth(fm.fighterDefinition.Health);
            }
        }
    }
}