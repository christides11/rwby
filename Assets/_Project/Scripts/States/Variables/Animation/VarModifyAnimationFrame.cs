using HnSF;
using UnityEngine;

namespace rwby
{
    [StateVariable("Animation/Modify Animation Frame")]
    public struct VarModifyAnimationFrame : IStateVariables
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

        public VarModifyType modifyType;
        public int[] animations;
        public int frame;

        public IStateVariables Copy()
        {
            return new VarModifyAnimationFrame()
            {
                frameRanges = frameRanges,
                condition = condition.Copy(),
                modifyType = modifyType,
                animations = animations,
                frame = frame
            };
        }
    }
}