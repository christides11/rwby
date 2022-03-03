using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ApplyGravityAsset : GravityAsset
    {
        public ApplyGravityBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ApplyGravityBehaviour>.Create(graph, template);
            return playable;
        }
    }
}