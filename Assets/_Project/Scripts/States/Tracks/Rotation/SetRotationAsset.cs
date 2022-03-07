using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SetRotationAsset : RotationAsset
    {
        public SetRotationBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SetRotationBehaviour>.Create(graph, template);
            return playable;
        }
    }
}