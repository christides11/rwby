using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SetMovementBehaviour : MovementBehaviour
    {
        public enum InputSource
        {
            stick,
            rotation
        }

        public InputSource inputSource;
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase wantedForce = new FighterBaseStatReferenceFloat();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;

            Vector3 vector = Vector3.zero;
            switch (inputSource)
            {
                case InputSource.stick:
                    vector = manager.GetMovementVector();
                    break;
                case InputSource.rotation:
                    vector = manager.transform.forward;
                    break;
            }

            force = vector * wantedForce.GetValue(manager);
        }
    }
}