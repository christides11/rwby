using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCanAirDash : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        [SelectImplementation((typeof(FighterStatReferenceBase<int>)))] [SerializeReference]
        public FighterStatReferenceIntBase maxAirDashes;

        public IConditionVariables Copy()
        {
            return new ConditionCanAirDash()
            {
                maxAirDashes = maxAirDashes == null ? null : (FighterStatReferenceIntBase)maxAirDashes.Copy()
            };
        }
    }
}