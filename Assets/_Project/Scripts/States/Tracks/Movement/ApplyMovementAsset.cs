using System;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ApplyMovementAsset : MovementAsset
    {
        public ApplyMovementBehaviour template;

        protected virtual void Awake()
        {
            template = new ApplyMovementBehaviour();
            template.forceSetType = ForceSetType.ADD;
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ApplyMovementBehaviour>.Create(graph, template);
            return playable;
        }
    }
}