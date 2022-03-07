using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ApplyJumpForceBehaviour : GravityBehaviour
    {
        public bool useValue = false;

        //[ShowIf("useValue"), AllowNesting]
        public float value = 0;
        
        //[HideIf("useValue"), AllowNesting]
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase maxJumpTime = new FighterBaseStatReferenceFloat();

        //[HideIf("useValue"), AllowNesting]
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase jumpHeight = new FighterBaseStatReferenceFloat();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;
            float apexTime = maxJumpTime.GetValue(manager) / 2.0f;
            force = useValue ? value : (2.0f * jumpHeight.GetValue(manager)) / apexTime;
        }
    }
}