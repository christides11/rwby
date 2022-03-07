using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace rwby
{
    [TrackClipType(typeof(MovementAsset))]
    public class MovementTrack : FighterTrack
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var p = ScriptPlayable<MovementMixerBehaviour>.Create(graph, inputCount);
            return p;
        }
    }
}