using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace rwby
{
    [TrackClipType(typeof(AnimationAsset))]
    public class AnimationTrack : FighterTrack
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<AnimationMixerBehaviour>.Create(graph, inputCount);
        }
    }
}