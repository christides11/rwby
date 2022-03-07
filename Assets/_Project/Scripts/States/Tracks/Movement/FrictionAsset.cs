using System;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    public class FrictionAsset : MovementAsset
    {
        public FrictionBehaviour template;

        protected virtual void Awake()
        {
            template = new FrictionBehaviour();
            template.forceSetType = ForceSetType.ADD;
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<FrictionBehaviour>.Create(graph, template);
            return playable;
        }
    }
}