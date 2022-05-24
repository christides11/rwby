using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "State", menuName = "rwby/statetimeline")]
    public class StateTimeline : HnSF.StateTimeline
    {
        public StateGroundedGroupType stateGroundedGroup;
        public bool allowBaseStateTransitions = true;
    }
}