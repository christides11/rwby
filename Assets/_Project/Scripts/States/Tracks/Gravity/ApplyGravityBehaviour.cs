using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ApplyGravityBehaviour : GravityBehaviour
    {
        public bool useValue = false;
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase value = new FighterBaseStatReferenceFloat();
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase maxJumpTime = new FighterBaseStatReferenceFloat();
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase jumpHeight = new FighterBaseStatReferenceFloat();
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase fallMultiplier = new FighterBaseStatReferenceFloat();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;

            switch (useValue)
            {
                case true:
                    force = -value.GetValue(manager) * manager.Runner.DeltaTime;
                    break;
                case false:
                    float apexTime = maxJumpTime.GetValue(manager) / 2.0f;
                    force = ((-2.0f * jumpHeight.GetValue(manager)) / Mathf.Pow(apexTime, 2)) 
                            * fallMultiplier.GetValue(manager)
                            * manager.Runner.DeltaTime;
                    break;
            }
        }
    }
}