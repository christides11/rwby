using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    public struct VarChangeStateList : IStateVariables
    {
        [System.Serializable]
        public struct StateListEntry
        {
            public int movesetID;
            [SelectImplementation(typeof(FighterStateReferenceBase))] [SerializeField, SerializeReference] [AllowNesting]
            public FighterStateReferenceBase state;
        }
        public int FunctionMap => (int)BaseStateFunctionEnum.NULL;
        public IConditionVariables Condition => condition;
        public IStateVariables[] Children => children;
        public Vector2[] FrameRanges
        {
            get => frameRanges;
            set => frameRanges = value;
        }
    
        [SerializeField] public Vector2[] frameRanges;
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference] 
        public IConditionVariables condition;

        public bool checkInputSequence;
        public bool checkCondition;

        public StateListEntry[] states;

        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}