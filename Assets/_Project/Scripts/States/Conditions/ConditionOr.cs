using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionOr: IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;
        
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference] 
        public IConditionVariables[] conditions;

        public IConditionVariables Copy()
        {
            var copy = new ConditionOr();
            if (conditions != null)
            {
                copy.conditions = new IConditionVariables[conditions.Length];
                for(int i = 0; i < conditions.Length; i++)
                {
                    copy.conditions[i] = conditions[i].Copy();
                }
            }
            return copy;
        }
    }
}