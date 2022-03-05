using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ApplyMovementBehaviour : MovementBehaviour
    {
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase baseAcceleration = new FighterBaseStatReferenceFloat();
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase AdditiveAcceleration = new FighterBaseStatReferenceFloat();
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase deceleration = new FighterBaseStatReferenceFloat();
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase minSpeed = new FighterBaseStatReferenceFloat();
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase maxSpeed = new FighterBaseStatReferenceFloat();
        
        [SelectImplementation((typeof(FighterStatReferenceBase<AnimationCurve>)))] [SerializeReference]
        public FighterStatReferenceAnimationCurveBase accelFromDotProduct = new FighterBaseStatReferenceAnimationCurve();
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;

            force = manager.FPhysicsManager.HandleMovement(baseAcceleration.GetValue(manager), AdditiveAcceleration.GetValue(manager), 
                deceleration.GetValue(manager), minSpeed.GetValue(manager), maxSpeed.GetValue(manager), accelFromDotProduct.GetValue(manager));
        }
    }
}