using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ApplyGravityAsset : GravityAsset
    {
        public ApplyGravityBehaviour template;

        public virtual void Awake()
        {
            template ??= new ApplyGravityBehaviour();
            template.forceSetType = ForceSetType.ADD;
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ApplyGravityBehaviour>.Create(graph, template);
            return playable;
        }
    }
}