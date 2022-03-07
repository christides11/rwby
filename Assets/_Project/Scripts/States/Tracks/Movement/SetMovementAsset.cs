using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SetMovementAsset : MovementAsset
    {
        public SetMovementBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SetMovementBehaviour>.Create(graph, template);
            return playable;
        }
    }
}