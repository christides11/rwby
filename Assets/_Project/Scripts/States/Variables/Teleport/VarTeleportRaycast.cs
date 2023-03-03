using HnSF;
using UnityEngine;

namespace rwby
{
    [StateVariable("Teleport/Raycast")]
    public struct VarTeleportRaycast : IStateVariables
    {
        public enum StartPointTypes
        {
            Self,
            Target
        }

        public enum RaycastDirSource
        {
            STICK,
            ROTATION,
            TARGET_VECTOR,
            CUSTOM
        }
        
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

        public RaycastDirSource raycastDirectionSource;
        public Vector3 direction;
        public float distance;
        public bool goToPosOnNoHit;
        public bool bypassInterpolation;
        public StartPointTypes startPoint;
        public float startUpOffset;

        public IStateVariables Copy()
        {
            return new VarTeleportRaycast()
            {
                raycastDirectionSource = raycastDirectionSource,
                direction = direction,
                distance = distance,
                goToPosOnNoHit = goToPosOnNoHit,
                bypassInterpolation = bypassInterpolation,
                startPoint = startPoint,
                startUpOffset = startUpOffset,
            };
        }
    }
}