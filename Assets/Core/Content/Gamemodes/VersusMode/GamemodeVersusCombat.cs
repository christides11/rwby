using Fusion;
using HnSF;

namespace rwby.core.versus
{
    public class GamemodeVersusCombat : NetworkBehaviour, IGamemodeCombat
    {
        public GamemodeVersus gamemode;
        
        public bool IsHitHurtboxValid(CustomHitbox attackerHitbox, Hurtbox attackeeHurtbox)
        {
            ITeamable attackerTeamable = attackerHitbox.ownerNetworkObject.GetComponent<ITeamable>();
            ITeamable attackeeTeamable = attackeeHurtbox.ownerNetworkObject.GetComponent<ITeamable>();

            if (attackerTeamable.GetTeam() == -1) return true;
            if (attackerTeamable.GetTeam() == attackeeTeamable.GetTeam()) return false;
            return true;
        }

        public bool IsHitHitboxValid(CustomHitbox attackerHitbox, CustomHitbox attackeeHitbox)
        {
            ITeamable attackerTeamable = attackerHitbox.ownerNetworkObject.GetComponent<ITeamable>();
            ITeamable attackeeTeamable = attackeeHitbox.ownerNetworkObject.GetComponent<ITeamable>();

            if (attackerTeamable.GetTeam() == -1) return true;
            if (attackerTeamable.GetTeam() == attackeeTeamable.GetTeam()) return false;
            return true;
        }
    }
}