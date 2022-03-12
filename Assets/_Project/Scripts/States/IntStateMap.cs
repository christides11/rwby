using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class IntStateMap
    {
        [SelectImplementation((typeof(FighterStateReferenceBase)))] [SerializeReference]
        public FighterStateReferenceBase state = new FighterCmnStateReference();
        public StateTimeline stateTimeline;
    }
}