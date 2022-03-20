using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ResetHitObjectsAsset : FighterStateAsset
    {
        public ResetHitObjectsBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.timelineClip = clipPassthrough;
            var playable = ScriptPlayable<ResetHitObjectsBehaviour>.Create(graph, template);
            return playable;
        }
    }
}