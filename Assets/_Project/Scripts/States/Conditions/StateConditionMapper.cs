using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class StateConditionMapper : HnSF.StateConditionMapperBase
    {
        public StateConditionMapper()
        {
            functions.Add((int)BaseStateConditionEnum.NONE, BaseStateConditionFunctions.NoCondition);
            /*
            functions.Add((int)BaseStateConditionEnum.MOVEMENT_MAGNITUDE, BaseStateConditionFunctions.MovementSqrMagnitude);
            functions.Add((int)BaseStateConditionEnum.GROUND_STATE, BaseStateConditionFunctions.GroundedState);
            functions.Add((int)BaseStateConditionEnum.FALL_SPEED, BaseStateConditionFunctions.FallSpeed);*/
        }
    }
}