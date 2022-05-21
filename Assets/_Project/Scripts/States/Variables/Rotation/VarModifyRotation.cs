using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    public struct VarModifyRotation : IStateVariables
    {
        public int FunctionMap => (int)BaseStateFunctionEnum.MODIFY_ROTATION;
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

        public VarModifyType modifyType;
        public VarRotateTowardsType rotateTowards;
        [ShowIf("rotateTowards", VarRotateTowardsType.custom)][AllowNesting]
        public Vector3 eulerAngle;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}