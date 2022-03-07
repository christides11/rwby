using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace rwby
{
    [TrackClipType(typeof(RotationAsset))]
    public class RotationTrack : FighterTrack
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<RotationMixerBehaviour>.Create(graph, inputCount);
        }
    }
}