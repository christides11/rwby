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

        public bool useTractionStat;
        public bool aerialTraction;
        public float traction;
        
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}