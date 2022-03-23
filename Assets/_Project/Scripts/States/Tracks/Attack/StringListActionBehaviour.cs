using HnSF;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class StringListActionBehaviour : FighterStateBehaviour
    {
        public enum StringListActionTypes
        {
            CLEAR,
            ADD
        }

        public StringListActionTypes actionType;
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = playerData as FighterManager;
            switch (actionType)
            {
                case StringListActionTypes.ADD:
                    manager.FCombatManager.AddMoveToString();
                    break;
                case StringListActionTypes.CLEAR:
                    manager.FCombatManager.ResetString();
                    break;
            }
        }
    }
}