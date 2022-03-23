using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class StringListActionAsset : FighterStateAsset
    {
        [SerializeField]
        public StringListActionBehaviour template;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<StringListActionBehaviour>.Create(graph, template);
            return playable;
        }
    }
}