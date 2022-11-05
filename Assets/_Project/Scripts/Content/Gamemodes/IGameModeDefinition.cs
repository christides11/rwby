using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    public abstract class IGameModeDefinition : IContentDefinition
    {
        public override string Name { get; }
        public override string Description { get; }

        public int minimumPlayers = 1;
        public int maximumPlayers = int.MaxValue;
        public int minimumTeams = 1;
        public int maximumTeams = 1;
        [FormerlySerializedAs("teams")] public TeamDefinition[] defaultTeams;

        public abstract GameObject GetGamemode();
    }
}