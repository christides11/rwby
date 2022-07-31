using System;
using System.Collections;
using System.Collections.Generic;
using HnSF;
using HnSF.Input;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [CreateAssetMenu(fileName = "State", menuName = "rwby/statetimeline")]
    public class StateTimeline : HnSF.StateTimeline, IBoxDefinitionCollection
    {
        public HitInfo[] HitboxInfo
        {
            get { return hitboxInfo; }
        }
        public ThrowInfo[] ThrowboxInfo
        {
            get { return throwboxInfo; }
        }
        public HurtboxInfo[] HurtboxInfo
        {
            get { return hurtboxInfo; }
        }

        [Header("State Info")]
        public StateGroundedGroupType stateGroundedGroup;
        public StateType stateType;
        [EnableIf("stateType", StateType.ATTACK)]
        public int maxUsesInString = -1;
        public bool allowBaseStateTransitions = true;

        [Header("Conditions")]
        [FormerlySerializedAs("stateInputSequence")] public InputSequence inputSequence;
        public bool inputSequenceAsHoldInputs;
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference]
        public IConditionVariables condition;

        [Header("Boxes")]
        [SerializeField] private HitInfo[] hitboxInfo;
        [SerializeField] private ThrowInfo[] throwboxInfo;
        [SerializeField] private HurtboxInfo[] hurtboxInfo;
    }
}