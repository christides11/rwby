using HnSF;
using UnityEngine;

namespace rwby
{
    public struct VarSetECB : IStateVariables
    {
        public int FunctionMap => (int)BaseStateFunctionEnum.SET_ECB;
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

        public float ecbCenter;
        public float ecbRadius;
        public float ecbHeight;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}