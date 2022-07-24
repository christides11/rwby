using HnSF;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [StateVariable("Gravity/Apply Gravity")]
    public struct VarApplyGravity : IStateVariables
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
        private int[] children;
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
        public IConditionVariables Condition => condition;

        private bool calculateValue => !useValue;
        public bool useValue;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference] [ShowIf("useValue")]
        public FighterStatReferenceFloatBase value;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference] [ShowIf("calculateValue")]
        public FighterStatReferenceFloatBase jumpHeight;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference] [ShowIf("calculateValue")]
        public FighterStatReferenceFloatBase jumpTime;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference] [FormerlySerializedAs("gravityMultiplier")]
        public FighterStatReferenceFloatBase multi;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase maxFallSpeed;
    }
}