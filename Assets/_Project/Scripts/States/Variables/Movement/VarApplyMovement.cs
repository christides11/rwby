using HnSF;
using UnityEngine;

namespace rwby
{
    public struct VarApplyMovement : IStateVariables
    {
        public enum InputSource
        {
            stick,
            rotation
        }

        public int FunctionMap => (int)BaseStateFunctionEnum.APPLY_MOVEMENT;
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
        public FighterStatReferenceFloatBase baseAccel;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase movementAccel;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase deceleration;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase minSpeed;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase maxSpeed;
        [SelectImplementation((typeof(FighterStatReferenceBase<AnimationCurve>)))] [SerializeReference]
        public FighterStatReferenceAnimationCurveBase accelerationFromDot;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}