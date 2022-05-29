using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    public struct VarModifyAnimationSet : IStateVariables
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
        public AnimationReference[] wantedAnimations;
        [ShowIf("modifyType", VarModifyType.SET)] public float fadeTime;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}