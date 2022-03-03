using HnSF.Sample.TDAction;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ClampGravityBehaviour : FighterStateBehaviour
    {
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeField, SerializeReference]
        public FighterStatReferenceFloatBase minValue = new FighterStatReferenceFloatBase();
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeField, SerializeReference]
        public FighterStatReferenceFloatBase maxValue = new FighterStatReferenceFloatBase();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;

            manager.FPhysicsManager.forceGravity =
                Mathf.Clamp(manager.FPhysicsManager.forceGravity, minValue.GetValue(manager), maxValue.GetValue(manager));
        }
    }
}