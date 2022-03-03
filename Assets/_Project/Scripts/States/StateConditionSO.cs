using HnSF;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "StateCondition", menuName = "rwby/statecondition")]
    public class StateConditionSO : ScriptableObject
    {
        [SelectImplementation(typeof(StateConditionBase))] [SerializeField, SerializeReference]
        public StateConditionBase conditon = new StateConditionBoolean();
    }
}