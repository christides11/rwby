using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ChangeStateBehaviour : FighterStateBehaviour
    {
        public bool checkStateCondition = false;
        
        [SelectImplementation((typeof(FighterStateReferenceBase)))] [SerializeReference]
        public FighterStateReferenceBase state = new FighterCmnStateReference();
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager cm = playerData as FighterManager;
            if ((cm.StateManager as FighterStateManager).markedForStateChange) return;
            if (conditon.IsTrue(cm) == false) return;
            (cm.StateManager as FighterStateManager).MarkForStateChange(state.GetState());
        }
    }
}