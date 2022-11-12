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

        public void FighterHealthChanged(FighterManager fm, int oldValue)
        {
            if (fm.HealthManager.Health <= 0 && oldValue > 0)
            {
                if (gamemode.GamemodeState != GameModeState.MATCH_IN_PROGRESS) return;
                fm.HandleDeath();
                if (Object.HasStateAuthority)
                {
                    var respawnPoint = gamemode.GetRespawnPosition(fm.FCombatManager.Team);
                    fm.FPhysicsManager.SetPosition(respawnPoint.transform.position, true);
                    fm.FPhysicsManager.SetRotation(respawnPoint.transform.eulerAngles, true);
                    fm.HandleRespawn();
                    GiveTeamPoint(fm.FCombatManager.GetTeam(), fm.lastHurtByTeam);
                }
                fm.HealthManager.SetHealth(fm.fighterDefinition.Health);
            }
        }

        private void GiveTeamPoint(int getTeam, int fmLastHurtByTeam)
        {
            gamemode.teamScores.Set(fmLastHurtByTeam, gamemode.teamScores[fmLastHurtByTeam] + 1);
        }
    }
}