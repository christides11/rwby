using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class MultiplyMovementAsset : MovementAsset
    {
        public MultiplyMovementBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<MultiplyMovementBehaviour>.Create(graph, template);
            return playable;
        }
    }
}