using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class FrictionBehaviour : MovementBehaviour
    {
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeField, SerializeReference]
        public FighterStatReferenceFloatBase friction = new FighterStatReferenceFloatBase();
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;

            Vector3 realFriction = manager.FPhysicsManager.forceMovement.normalized * friction.GetValue(manager);

            force.x = manager.FPhysicsManager.GetFrictionValue(manager.FPhysicsManager.forceMovement.x, realFriction.x);
            force.z = manager.FPhysicsManager.GetFrictionValue(manager.FPhysicsManager.forceMovement.z, realFriction.z);
        }
    }
}