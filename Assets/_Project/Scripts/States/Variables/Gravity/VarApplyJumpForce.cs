using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    public struct VarApplyJumpForce : IStateVariables
    {
        public int FunctionMap => (int)BaseStateFunctionEnum.APPLY_JUMP_FORCE;
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

        public bool useValue;
        [ShowIf("useValue")] [AllowNesting]
        public float value;
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase maxJumpTime;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase jumpHeight;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}