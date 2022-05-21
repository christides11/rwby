using HnSF;
using UnityEngine;

namespace rwby
{
    public struct VarApplyTraction : IStateVariables
    {
        public int FunctionMap => (int)BaseStateFunctionEnum.APPLY_TRACTION;
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

        [SelectImplementation(typeof(FighterStatReferenceFloatBase))] [SerializeField, SerializeReference]
        public FighterStatReferenceFloatBase traction;

        public bool applyMovement;
        public bool applyGravity;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}