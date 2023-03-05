using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [StateVariable("Movement/Apply Movement")]
    public struct VarApplyMovement : IStateVariables
    {
        public string name;
        public string Name
        {
            get => name;
            set => name = value;
        }
        [SerializeField, HideInInspector] private int id;
        public int ID
        {
            get => id;
            set => id = value;
        }
        [SerializeField, HideInInspector] private int parent;
        public int Parent
        {
            get => parent;
            set => parent = value;
        }
        [SerializeField, HideInInspector] private int[] children;
        public int[] Children
        {
            get => children;
            set => children = value;
        }
        [SerializeField] public Vector2Int[] frameRanges;
        public Vector2Int[] FrameRanges
        {
            get => frameRanges;
            set => frameRanges = value;
        }
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference]
        public IConditionVariables condition;
         public IConditionVariables Condition { get => condition; set => condition = value; }
        public bool RunDuringHitstop { get => runDuringHitstop; set => runDuringHitstop = value; }
        public bool runDuringHitstop;

        public VarInputSourceType inputSource;
        public bool normalizeInputSource;
        public bool useRotationIfInputZero;
        public bool useSlope;
        
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

        //[ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeMinClamp;
        //[ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeMaxClamp;
        //[ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeDivi;
        //[ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeMulti;
        //[ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeMultiMin;
        public float slopeMultiMax;
        //[ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeinputModi;

        public IStateVariables Copy()
        {
            return new VarApplyMovement()
            {
                inputSource = inputSource,
                normalizeInputSource = normalizeInputSource,
                useRotationIfInputZero = useRotationIfInputZero,
                useSlope = useSlope,
                baseAccel = (FighterStatReferenceFloatBase)baseAccel?.Copy(),
                movementAccel = (FighterStatReferenceFloatBase)movementAccel?.Copy(),
                deceleration = (FighterStatReferenceFloatBase)deceleration?.Copy(),
                minSpeed = (FighterStatReferenceFloatBase)minSpeed?.Copy(),
                maxSpeed = (FighterStatReferenceFloatBase)maxSpeed?.Copy(),
                accelerationFromDot = (FighterStatReferenceAnimationCurveBase)accelerationFromDot?.Copy(),
                slopeMinClamp = slopeMinClamp,
                slopeMaxClamp = slopeMaxClamp,
                slopeDivi = slopeDivi,
                slopeMulti = slopeMultiMin,
                slopeMultiMax = slopeMultiMax,
                slopeMultiMin = slopeMultiMin,
                slopeinputModi = slopeinputModi,
            };
        }
    }
}