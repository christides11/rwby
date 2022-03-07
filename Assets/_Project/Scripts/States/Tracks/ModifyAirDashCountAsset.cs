using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ModifyAirDashCountAsset : FighterStateAsset
    {
        public ModifyAirDashCountBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ModifyAirDashCountBehaviour>.Create(graph, template);
            return playable;
        }
    }
}