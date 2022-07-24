using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [StateVariable("State/Change State List")]
    public struct VarChangeStateList : IStateVariables
    {
        [System.Serializable]
        public struct StateListEntry
        {
            public int movesetID;
            [SelectImplementation(typeof(FighterStateReferenceBase))] [SerializeField, SerializeReference] [AllowNesting]
            public FighterStateReferenceBase state;
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

        public bool checkInputSequence;
        public bool checkCondition;

        public StateListEntry[] states;
    }
}