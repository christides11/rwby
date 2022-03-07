using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class MultiplyGravityBehaviour : GravityBehaviour
    {
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase multiplier = new FighterBaseStatReferenceFloat();
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;
            
            force = (manager.FPhysicsManager.forceGravity * multiplier.GetValue(manager)) - manager.FPhysicsManager.forceGravity;
        }
    }
}