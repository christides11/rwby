using System;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class RotateTowardsAsset : RotationAsset
    {
        public RotateTowardsBehaviour template;

        private void Awake()
        {
            template = new RotateTowardsBehaviour();
            template.rotationSetType = ForceSetType.ADD;
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<RotateTowardsBehaviour>.Create(graph, template);
            return playable;
        }
    }
}