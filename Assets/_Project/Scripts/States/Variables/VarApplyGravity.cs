using HnSF;
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

        public bool useMaxFallSpeedStat;
        public bool useGravityStat;
        public float maxFallSpeed;
        public float gravity;
        
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}