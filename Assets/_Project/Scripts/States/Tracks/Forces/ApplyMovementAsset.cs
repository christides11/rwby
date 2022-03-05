using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ApplyMovementAsset : MovementAsset
    {
        public ApplyMovementBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ApplyMovementBehaviour>.Create(graph, template);
            return playable;
        }
    }
}