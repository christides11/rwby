using Fusion;

namespace rwby
{
    [System.Serializable]
    public struct TrainingCPUSettingsDefinition : INetworkStruct
    {
        public int behaviour;
        public int stance;
        public int block;
        public int blockDirectionSwitch;
    }
}