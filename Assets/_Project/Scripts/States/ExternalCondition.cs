using System.Collections;
using System.Collections.Generic;
using HnSF;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "External Condition", menuName = "rwby/ExternalCondition")]
    public class ExternalCondition : ScriptableObject
    {
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference]
        public IConditionVariables condition;
    }
}