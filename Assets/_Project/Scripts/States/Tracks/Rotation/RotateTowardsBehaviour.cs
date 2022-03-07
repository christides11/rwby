using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class RotateTowardsBehaviour : RotationBehaviour
    {
        public enum RotateTowardsEnum
        {
            stick,
            movement
        }

        public RotateTowardsEnum rotateTowards = RotateTowardsEnum.stick;
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase rotationSpeed = new FighterBaseStatReferenceFloat();
        
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
            euler = manager.GetVisualRotation(movement.normalized, rotationSpeed.GetValue(manager));
        }
    }
}