using HnSF;
using UnityEngine;

namespace rwby
{
    public struct VarChangeState : IStateVariables
    {
        public int FunctionMap => (int)BaseStateFunctionEnum.CHANGE_STATE;
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

        public int stateMovesetID;
        [SelectImplementation(typeof(FighterStateReferenceBase))] [SerializeField, SerializeReference]
        public FighterStateReferenceBase state;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}