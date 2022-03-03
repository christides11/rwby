using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ApplyJumpForceAsset : GravityAsset
    {
        public new ApplyJumpForceBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ApplyJumpForceBehaviour>.Create(graph, template);
            return playable;
        }
    }
}