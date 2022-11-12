using System.Collections.Generic;

namespace rwby
{
    public interface ITeamScoreProvider
    {
        public IEnumerable<int> TeamScores { get; }
        public int MaxScore { get; }
    }
}