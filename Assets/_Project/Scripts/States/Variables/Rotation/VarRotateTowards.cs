using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    public struct VarRotateTowards : IStateVariables
    {
        public int FunctionMap => (int)BaseStateFunctionEnum.ROTATE_TOWARDS;
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
        
        public VarRotateTowardsType rotateTowards;
        [ShowIf("rotateTowards", VarRotateTowardsType.custom)][AllowNesting]
        public Vector3 eulerAngle;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase rotationSpeed;
        
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}