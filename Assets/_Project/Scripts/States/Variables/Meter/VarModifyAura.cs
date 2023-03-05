using HnSF;
using UnityEngine;

namespace rwby
{
    [StateVariable("Meter/Aura")]
    public struct VarModifyAura : IStateVariables
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

        public VarTargetType targetType;
        
        public VarModifyType modifyType;
        public bool valueIsPercentage;
        [SelectImplementation((typeof(FighterStatReferenceBase<int>)))] [SerializeReference]
        public FighterStatReferenceIntBase value;

        public IStateVariables Copy()
        {
            return new VarModifyAura()
            {
                targetType = targetType,
                modifyType = modifyType,
                valueIsPercentage = valueIsPercentage,
                value = (FighterStatReferenceIntBase)value?.Copy()
            };
        }
    }
}