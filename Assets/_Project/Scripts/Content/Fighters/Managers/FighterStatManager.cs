using Fusion;

namespace rwby
{
    public class FighterStatManager : NetworkBehaviour
    {
        private FighterStats fs;
        
        public FighterStats GetFighterStats()
        {
            return fs;
        }

        public void SetupStats(FighterStats cmnStats)
        {
            fs = cmnStats;
        }
    }
}