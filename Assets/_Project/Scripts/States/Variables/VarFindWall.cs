using HnSF;
using UnityEngine;

namespace rwby
{
    [StateVariable("Wall/Find Wall")]
    public struct VarFindWall : IStateVariables
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
        
        public VarSetMovement.InputSource inputSource;
        public bool normalizeInputSource;
        public bool useRotationIfInputZero;
        public float maxDistance;

        public bool checkForward;
        public bool checkBackward;
        public bool checkSide;

        public bool clearWallIfNotFound;
    }
}