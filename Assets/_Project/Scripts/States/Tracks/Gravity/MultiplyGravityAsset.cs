using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class MultiplyGravityAsset : GravityAsset
    {
        public MultiplyGravityBehaviour template;

        public virtual void Awake()
        {
            template = new MultiplyGravityBehaviour();
            template.forceSetType = ForceSetType.ADD;
        }
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<MultiplyGravityBehaviour>.Create(graph, template);
            return playable;
        }
    }
}