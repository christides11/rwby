using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [StateVariable("Rotation/Rotate Towards")]
    public struct VarRotateTowards : IStateVariables
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
        public IConditionVariables Condition => condition;
        
        public VarRotateTowardsType rotateTowards;
        [ShowIf("rotateTowards", VarRotateTowardsType.custom)][AllowNesting]
        public Vector3 eulerAngle;
        public bool rotateTowardsTarget;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase rotationSpeed;
    }
}