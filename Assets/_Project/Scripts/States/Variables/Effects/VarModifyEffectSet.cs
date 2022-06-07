using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    public struct VarModifyEffectSet : IStateVariables
    {
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

        public VarModifyType modifyType;
        public EffectReference[] wantedEffects;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}