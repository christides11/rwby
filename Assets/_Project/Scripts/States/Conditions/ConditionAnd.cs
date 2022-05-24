using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionAnd: IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;
        
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference] 
        public IConditionVariables[] conditions;
    }
}