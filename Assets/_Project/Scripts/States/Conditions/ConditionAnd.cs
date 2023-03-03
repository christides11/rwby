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

        public IConditionVariables Copy()
        {
            ConditionAnd c = new ConditionAnd();

            if(conditions != null)
            {
                c.conditions = new IConditionVariables[conditions.Length];
                for(int i = 0; i < c.conditions.Length; i++)
                {
                    c.conditions[i] = conditions[i]?.Copy();
                }
            }

            return c;
        }
    }
}