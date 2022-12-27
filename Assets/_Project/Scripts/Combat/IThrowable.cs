using UnityEngine;
using Fusion;

namespace rwby
{
    public interface IThrowable
    {
        bool throwLocked { get; set; }
        NetworkObject thrower { get; set; }
        int ThrowTechTimer { get; set; }
        
        public void ThroweeInitilization(NetworkObject thrower);
        public void SetThroweePosition(Vector3 position);
        public void SetThroweeRotation(Vector3 rotation);
    }
}