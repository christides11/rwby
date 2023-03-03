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
         public IConditionVariables Condition { get => condition; set => condition = value; }
        
        public VarInputSourceType inputSource;
        public bool normalizeInputSource;
        public bool useRotationIfInputZero;
        public float inputSourceOffset;

        public float minAngle;
        public float maxAngle;

        public int raycastCount;
        public float startAngleOffset;
        public bool angleBasedOnWallDir;

        public bool clearWallIfNotFound;

        public float rayDistance;

        public IStateVariables Copy()
        {
            return new VarFindWall()
            {
                inputSource = inputSource,
                normalizeInputSource = normalizeInputSource,
                useRotationIfInputZero = useRotationIfInputZero,
                inputSourceOffset = inputSourceOffset,
                minAngle = minAngle,
                maxAngle = maxAngle,
                raycastCount = raycastCount,
                startAngleOffset = startAngleOffset,
                angleBasedOnWallDir = angleBasedOnWallDir,
                clearWallIfNotFound = clearWallIfNotFound,
                rayDistance = rayDistance,
            };
        }
    }
}