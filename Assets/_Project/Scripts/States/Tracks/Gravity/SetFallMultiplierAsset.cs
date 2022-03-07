using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SetFallMultiplierAsset : FighterStateAsset
    {
        public SetFallMultiplierBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SetFallMultiplierBehaviour>.Create(graph, template);
            return playable;
        }
    }
}