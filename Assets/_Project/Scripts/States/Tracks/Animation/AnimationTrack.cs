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
            var clips = GetClips();
            foreach(var clip in clips)
            {
                var loopClip = clip.asset as AnimationAsset;
                loopClip.clipPassthrough = clip;
            }

            return base.CreateTrackMixer(graph, go, inputCount);
            //return ScriptPlayable<AnimationMixerBehaviour>.Create(graph, inputCount);
        }
    }
}