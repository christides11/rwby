using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SetECBAsset : FighterStateAsset
    {
        public SetECBBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.timelineClip = clipPassthrough;
            var playable = ScriptPlayable<SetECBBehaviour>.Create(graph, template);
            return playable;
        }
    }
}