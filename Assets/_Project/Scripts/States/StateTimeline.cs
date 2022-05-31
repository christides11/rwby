using System.Collections;
using System.Collections.Generic;
using HnSF;
using HnSF.Input;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [CreateAssetMenu(fileName = "State", menuName = "rwby/statetimeline")]
    public class StateTimeline : HnSF.StateTimeline
    {
        public StateGroundedGroupType stateGroundedGroup;
        public StateType stateType;
        public int maxUsesInString = -1;
        public bool allowBaseStateTransitions = true;

        [FormerlySerializedAs("stateInputSequence")] public InputSequence inputSequence;

        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference]
        public IConditionVariables condition;
    }
}