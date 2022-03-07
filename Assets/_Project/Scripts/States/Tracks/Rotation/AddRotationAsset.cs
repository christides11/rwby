using System;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class AddRotationAsset : RotationAsset
    {
        public AddRotationBehaviour template;

        protected void Awake()
        {
            template ??= new AddRotationBehaviour();
            template.rotationSetType = ForceSetType.ADD;
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<AddRotationBehaviour>.Create(graph, template);
            return playable;
        }
    }
}