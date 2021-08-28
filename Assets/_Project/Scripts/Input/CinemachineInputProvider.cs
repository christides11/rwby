using UnityEngine;

namespace rwby
{
    public class CinemachineInputProvider : MonoBehaviour, Cinemachine.AxisState.IInputAxisProvider
    {
        public Vector2 input;

        public float GetAxisValue(int axis)
        {
            switch (axis)
            {
                case 0: return input.x;
                case 1: return input.y;
            }
            return 0;
        }
    }
}