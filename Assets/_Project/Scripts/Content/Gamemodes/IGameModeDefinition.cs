using UnityEngine;

namespace rwby
{
    public abstract class IGameModeDefinition : IContentDefinition
    {
        public override string Name { get; }
        public override string Description { get; }

        public int minimumPlayers = 0;
        public int maximumPlayers = int.MaxValue;
        public int minimumTeams;
        public int maximumTeams;
        public TeamDefinition defaultTeam;
        public TeamDefinition[] teams;

        public abstract GameObject GetGamemode();
    }
}