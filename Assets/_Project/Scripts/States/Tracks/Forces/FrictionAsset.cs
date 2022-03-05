using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    public class FrictionAsset : MovementAsset
    {
        public FrictionBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<FrictionBehaviour>.Create(graph, template);
            return playable;
        }
    }
}