using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ApplyMovementBehaviour : MovementBehaviour
    {
        public enum InputSource
        {
            movementVector,
            rotation
        }

        public InputSource inputSource;
        
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

            Vector3 vector = Vector3.zero;
            switch (inputSource)
            {
                case InputSource.movementVector:
                    vector = manager.GetMovementVector();
                    break;
                case InputSource.rotation:
                    vector = manager.transform.forward;
                    break;
            }
            
            force = manager.FPhysicsManager.HandleMovement(vector, baseAcceleration.GetValue(manager), AdditiveAcceleration.GetValue(manager), 
                deceleration.GetValue(manager), minSpeed.GetValue(manager), maxSpeed.GetValue(manager), accelFromDotProduct.GetValue(manager));
        }
    }
}