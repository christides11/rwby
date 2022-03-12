using HnSF;
using HnSF.Fighters;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace rwby
{
    [TrackBindingType(typeof(FighterManager))]
    public class FighterTrack : HnSF.FighterTrack
    {
        
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var clips = GetClips();
            foreach (var clip in clips)
            {
                var fClip = clip.asset as FighterStateAsset;
                fClip.clipPassthrough = clip;
            }

            return base.CreateTrackMixer(graph, go, inputCount);
            //return ScriptPlayable<FighterStateBehaviour>.Create(graph, inputCount);
        }
    }
}