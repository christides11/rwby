using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SetRotationBehaviour : RotationBehaviour
    {
        public enum RotateTowardsEnum
        {
            stick,
            movement
        }

        public RotateTowardsEnum rotateTowards = RotateTowardsEnum.stick;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;
            Vector3 movement = Vector3.zero;
            switch (rotateTowards)
            {
                case RotateTowardsEnum.stick:
                    movement = manager.GetMovementVector();
                    break;
                case RotateTowardsEnum.movement:
                    movement = manager.FPhysicsManager.forceMovement;
                    break;
            }
            if (movement.sqrMagnitude == 0) return;
            euler = movement.normalized;
        }
    }
}