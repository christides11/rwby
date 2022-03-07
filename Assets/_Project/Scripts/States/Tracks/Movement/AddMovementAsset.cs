using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class AddMovementAsset : MovementAsset
    {
        public AddMovementBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<AddMovementBehaviour>.Create(graph, template);
            return playable;
        }
    }
}