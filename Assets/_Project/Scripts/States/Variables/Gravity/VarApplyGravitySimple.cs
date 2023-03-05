using HnSF;
using UnityEngine;

namespace rwby
{
    [StateVariable("Gravity/Apply Gravity (Simple)")]
    public struct VarApplyGravitySimple : IStateVariables
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
        [SelectImplementation(typeof(IConditionVariables))]
        [SerializeField, SerializeReference]
        public IConditionVariables condition;
         public IConditionVariables Condition { get => condition; set => condition = value; }
        public bool RunDuringHitstop { get => runDuringHitstop; set => runDuringHitstop = value; }
        public bool runDuringHitstop;

        public bool affectedByGravityMulti;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))]
        [SerializeReference]
        public FighterStatReferenceFloatBase value;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))]
        [SerializeReference]
        public FighterStatReferenceFloatBase multi;

        public bool useCurve;
        public AnimationCurve applyCurve;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))]
        [SerializeReference]
        public FighterStatReferenceFloatBase maxFallSpeed;

        public IStateVariables Copy()
        {
            return new VarApplyGravitySimple()
            {
                affectedByGravityMulti = affectedByGravityMulti,
                value = (FighterStatReferenceFloatBase)value?.Copy(),
                multi = (FighterStatReferenceFloatBase)multi?.Copy(),
                useCurve = useCurve,
                applyCurve = applyCurve,
                maxFallSpeed = (FighterStatReferenceFloatBase)maxFallSpeed?.Copy(),
            };
        }
    }
}