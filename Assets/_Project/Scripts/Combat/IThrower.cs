using Fusion;

namespace rwby
{
    public interface IThrower
    {
        NetworkArray<NetworkObject> throwees { get; }
        public bool IsThroweeValid(CustomHitbox attackerThrowbox, Throwablebox attackeeThrowablebox);
        public void ThrowerInitilization(NetworkObject throwee);
        public void OnThrowTeched(NetworkObject throwee);
        public void ReleaseThrowee(NetworkObject throwee);
    }
}