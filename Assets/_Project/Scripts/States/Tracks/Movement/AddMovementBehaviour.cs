using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class AddMovementBehaviour : MovementBehaviour
    {
        public enum InputSource
        {
            movementVector,
            rotation
        }

        public InputSource inputSource;
        public bool normalize = false;
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase addForce = new FighterBaseStatReferenceFloat();
        
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

            if (vector == Vector3.zero) return;
            if(normalize) vector.Normalize();

            force = vector * addForce.GetValue(manager);
        }
    }
}