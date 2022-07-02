using HnSF;
using UnityEngine;

namespace rwby
{
    public struct VarCreateProjectile : IStateVariables
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

        public ProjectileBase projectile;
        public Vector3 positionOffset;
        public Vector3 rotation;
        public Vector3 scale;

        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}