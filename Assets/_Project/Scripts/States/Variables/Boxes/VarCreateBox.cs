using HnSF;
using HnSF.Combat;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    public struct VarCreateBox : IStateVariables
    {
        public int FunctionMap => (int)BaseStateFunctionEnum.NULL;
        public IConditionVariables Condition => condition;
        public IStateVariables[] Children => children;

        public Vector2[] FrameRanges
        {
            get => frameRanges;
            set => frameRanges = value;
        }
    
        [SerializeField] public Vector2[] frameRanges;
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference] 
        public IConditionVariables condition;
        
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
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}