using HnSF;
using UnityEngine;

namespace rwby
{
    public struct VarModifyAirDashCount : IStateVariables
    {
        public int FunctionMap => (int)BaseStateFunctionEnum.MODIFY_AIR_DASH_COUNT;
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
        public int value;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}