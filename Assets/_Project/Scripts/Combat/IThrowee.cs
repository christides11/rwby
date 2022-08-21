using Fusion;
using UnityEngine;

namespace rwby
{
    public interface IThrowee
    {
        public void ThroweeInitilization(NetworkObject thrower);
        public void SetThroweePosition(Vector3 position);
        public void SetThroweeRotation(Vector3 rotation);
    }
}