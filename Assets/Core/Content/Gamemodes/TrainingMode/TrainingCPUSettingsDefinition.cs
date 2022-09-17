using Fusion;
using rwby.core.training;

namespace rwby
{
    [System.Serializable]
    public struct TrainingCPUSettingsDefinition : INetworkStruct
    {
        //BASICS
        public int status;
        public int guard;
        public int guardTiming;
        public int guardDirection;
        public int techThrow;
        public int counterHit;
        public int shield;
        
        // RECOVERY
        public int staggerRecovery;
        public int groundRecovery;
        public int airRecovery;
        
        // GUAGES
        public int lifeGuage;
        public int auraGuage;
        
        // COUNTER-HIT
        public int afterBlock;
        public int afterHit;
    }
}