using Fusion;

namespace rwby
{
    public enum CameraShakeStrength
    {
        None,
        Low,
        Medium,
        High
    }
    
    public struct CmaeraShakeDefinition : INetworkStruct
    {
        public CameraShakeStrength shakeStrength;
        public int startFrame;
        public int endFrame;
    }
}