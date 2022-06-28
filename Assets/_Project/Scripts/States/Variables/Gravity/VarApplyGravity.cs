using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    public struct VarApplyGravity : IStateVariables
    {
        public int FunctionMap => (int)BaseStateFunctionEnum.APPLY_GRAVITY;
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

        private bool calculateValue => !useValue;
        public bool useValue;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference] [ShowIf("calculateValue")] [AllowNesting]
        public FighterStatReferenceFloatBase jumpHeight;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference] [ShowIf("calculateValue")] [AllowNesting]
        public FighterStatReferenceFloatBase jumpTime;
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference] [ShowIf("useValue")] [AllowNesting]
        public FighterStatReferenceFloatBase value;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference] [AllowNesting]
        public FighterStatReferenceFloatBase gravityMultiplier;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference] [AllowNesting]
        public FighterStatReferenceFloatBase maxFallSpeed;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}