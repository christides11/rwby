using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [StateVariable("Stun/Modify Blockstun")]
    public struct VarModifyBlockstun : IStateVariables
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

        public VarModifyType modifyType;
        public int value;
        public bool clamp;
        [ShowIf("clamp")] public int clampMin;
        [ShowIf("clamp")] public int clampMax;

        public IStateVariables Copy()
        {
            return new VarModifyBlockstun()
            {
                modifyType = modifyType,
                value = value,
                clamp = clamp,
                clampMin = clampMin,
                clampMax = clampMax,
            };
        }
    }
}