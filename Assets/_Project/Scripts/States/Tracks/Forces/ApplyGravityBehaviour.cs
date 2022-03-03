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
        public bool checkFallMultiplier = false;

        [ShowIf("useValue"), AllowNesting]
        public float value = 0;
        
        //[HideIf("useValue"), AllowNesting]
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase maxJumpTime = new FighterBaseStatReferenceFloat();

        //[HideIf("useValue"), AllowNesting]
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase jumpHeight = new FighterBaseStatReferenceFloat();

        public float fallMultiplier = 1.0f;
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;

            switch (useValue)
            {
                case true:
                    force = value;
                    break;
                case false:
                    float apexTime = maxJumpTime.GetValue(manager) / 2.0f;
                    force = ((-2.0f * jumpHeight.GetValue(manager)) / Mathf.Pow(apexTime, 2)) * manager.Runner.DeltaTime * fallMultiplier;
                    break;
            }
        }
    }
}