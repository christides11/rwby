using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class FrictionBehaviour : MovementBehaviour
    {
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeField, SerializeReference]
        public FighterStatReferenceFloatBase friction = new FighterBaseStatReferenceFloat();
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;

            Vector3 forceMovement = manager.FPhysicsManager.forceMovement;
            bool xApprox = Mathf.Approximately(forceMovement.x, 0);
            bool zApprox = Mathf.Approximately(forceMovement.z, 0);
            if (xApprox) forceMovement.x = 0;
            if (zApprox) forceMovement.z = 0;
            manager.FPhysicsManager.forceMovement = forceMovement;
            if (xApprox && zApprox) return;
            
            Vector3 realFriction = forceMovement.normalized * friction.GetValue(manager);

            force.x = manager.FPhysicsManager.GetFrictionValue(forceMovement.x, Mathf.Abs(realFriction.x));
            force.z = manager.FPhysicsManager.GetFrictionValue(forceMovement.z, Mathf.Abs(realFriction.z));
        }
    }
}