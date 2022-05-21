using HnSF;
using UnityEngine;

namespace rwby
{
    public struct VarSetMovement : IStateVariables
    {
        public enum InputSource
        {
            stick,
            rotation
        }
        
        public int FunctionMap => (int)BaseStateFunctionEnum.SET_MOVEMENT;
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

        public InputSource inputSource;
        public bool normalizeInputSource;
        public bool useRotationIfInputZero;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase force;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}