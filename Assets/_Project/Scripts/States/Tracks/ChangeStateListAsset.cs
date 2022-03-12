using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ChangeStateListAsset : FighterStateAsset
    {
        public ChangeStateListBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ChangeStateListBehaviour>.Create(graph, template);
            return playable;
        }
    }
}