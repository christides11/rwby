using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class BoxAsset : FighterStateAsset
    {
        public BoxBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.timelineClip = clipPassthrough;
            var playable = ScriptPlayable<BoxBehaviour>.Create(graph, template);
            return playable;
        }
    }
}