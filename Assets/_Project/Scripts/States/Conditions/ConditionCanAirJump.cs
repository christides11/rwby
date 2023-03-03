using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionCanAirJump : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        [SelectImplementation((typeof(FighterStatReferenceBase<int>)))] [SerializeReference]
        public FighterStatReferenceIntBase maxAirJumps;

        public IConditionVariables Copy()
        {
            return new ConditionCanAirJump()
            {
                // TODO:
            };
        }
    }
}