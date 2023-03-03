using HnSF;
using HnSF.Combat;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    [StateVariable("Boxes/Create Box")]
    public struct VarCreateBox : IStateVariables
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
        
        private bool IsRectangle => shape == BoxShape.Rectangle;
        public FighterBoxType boxType;
        public int attachedTo;
        public BoxShape shape;
        public Vector3 offset;
        [ShowIf("IsRectangle")]
        public Vector3 boxExtents;
        [HideIf("IsRectangle")]
        public float radius;
        public int definitionIndex;

        public IStateVariables Copy()
        {
            return new VarCreateBox()
            {
                boxType = boxType,
                attachedTo = attachedTo,
                shape = shape,
                offset = offset,
                boxExtents = boxExtents,
                radius = radius,
                definitionIndex = definitionIndex
            };
        }
    }
}