using HnSF;
using UnityEngine;

namespace rwby
{
    [StateVariable("State/Change State")]
    public struct VarChangeState : IStateVariables
    {
        
        public string name;
        public string Name
        {
            get => name;
            set => name = value;
        }
        [SerializeField, HideInInspector] private int id;
        public int ID
        {
            get => id;
            set => id = value;
        }
        [SerializeField, HideInInspector] private int parent;
        public int Parent
        {
            get => parent;
            set => parent = value;
        }
        [SerializeField, HideInInspector] private int[] children;
        public int[] Children
        {
            get => children;
            set => children = value;
        }
        [SerializeField] public Vector2Int[] frameRanges;
        public Vector2Int[] FrameRanges
        {
            get => frameRanges;
            set => frameRanges = value;
        }
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference]
        public IConditionVariables condition;
         public IConditionVariables Condition { get => condition; set => condition = value; }

        public bool ignoreStateConditions;
        public bool ignoreAirtimeCheck;
        public bool ignoreStringUseCheck;
        public bool ignoreAuraRequirement;
        public bool checkInputSequence;
        public bool checkCondition;
        public bool overrideStateChange;

        public VarTargetType targetType;
        
        public int stateMovesetID;
        [SelectImplementation(typeof(FighterStateReferenceBase))] [SerializeField, SerializeReference]
        public FighterStateReferenceBase state;
        public int frame;

        public IStateVariables Copy()
        {
            return new VarChangeState()
            {
                ignoreStateConditions = ignoreStateConditions,
                ignoreAirtimeCheck = ignoreAirtimeCheck,
                ignoreStringUseCheck = ignoreStringUseCheck,
                ignoreAuraRequirement = ignoreAuraRequirement,
                checkInputSequence = checkInputSequence,
                checkCondition = checkCondition,
                overrideStateChange = overrideStateChange,
                targetType = targetType,
                stateMovesetID = stateMovesetID,
                state = state == null ? null : (FighterStateReferenceBase)state?.Copy(),
                frame = frame
            };
        }
    }
}