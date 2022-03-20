using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SnapECBAsset : FighterStateAsset
    {
        public SnapECBBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.timelineClip = clipPassthrough;
            var playable = ScriptPlayable<SnapECBBehaviour>.Create(graph, template);
            return playable;
        }
    }
}