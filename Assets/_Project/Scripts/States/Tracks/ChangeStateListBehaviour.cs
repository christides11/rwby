using HnSF;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class ChangeStateListBehaviour : FighterStateBehaviour
    {
        public StateReferenceList referenceList;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager cm = playerData as FighterManager;
            if ((cm.StateManager as FighterStateManager).markedForStateChange) return;
            if (conditon.IsTrue(cm) == false) return;
            for (int i = 0; i < referenceList.stateList.Length; i++)
            {
                StateTimeline s = cm.FStateManager.GetState(referenceList.stateList[i].GetState()) as StateTimeline;
                if (s.conditon.IsTrue(cm) == false) continue;
                (cm.StateManager as FighterStateManager).MarkForStateChange(referenceList.stateList[i].GetState());
                return;
            }
        }
    }
}