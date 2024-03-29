using HnSF;
using UnityEngine;

namespace rwby
{
    [StateVariable("Movement/Move Towards Magnitude")]
    public struct VarMoveTowardsMagnitude : IStateVariables
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


        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase force;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase distance;
        public bool applyMovement;
        public bool applyGravity;

        public IStateVariables Copy()
        {
            return new VarMoveTowardsMagnitude()
            {
                force = (FighterStatReferenceFloatBase)force?.Copy(),
                distance = (FighterStatReferenceFloatBase)distance?.Copy(),
                applyMovement = applyMovement,
                applyGravity = applyGravity,
            };
        }
    }
}