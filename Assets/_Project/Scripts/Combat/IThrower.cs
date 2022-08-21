using Fusion;

namespace rwby
{
    public interface IThrower
    {
        public bool IsThroweeValid(CustomHitbox attackerThrowbox, Throwablebox attackeeThrowablebox);
        public void ThrowerInitilization(NetworkObject throwee);
    }
}