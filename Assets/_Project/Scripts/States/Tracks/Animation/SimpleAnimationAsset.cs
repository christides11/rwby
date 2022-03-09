using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SimpleAnimationAsset : AnimationAsset
    {
        public SimpleAnimationBehaviour template;

        public virtual void Awake()
        {
            template ??= new SimpleAnimationBehaviour();
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SimpleAnimationBehaviour>.Create(graph, template);
            return playable;
        }
    }
}