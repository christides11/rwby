using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class StateFunctionMapper : HnSF.StateFunctionMapperBase
    {
        public StateFunctionMapper()
        {
            functions.Add((int)BaseStateFunctionEnum.NULL, BaseStateFunctions.Null);
            functions.Add((int)BaseStateFunctionEnum.CHANGE_STATE, BaseStateFunctions.ChangeState);
            //functions.Add((int)BaseStateFunctionEnum.APPLY_GRAVITY, BaseStateFunctions.ApplyGravity);
            functions.Add((int)BaseStateFunctionEnum.APPLY_TRACTION, BaseStateFunctions.ApplyTraction);
            //functions.Add((int)BaseStateFunctionEnum.SET_FALL_SPEED, BaseStateFunctions.SetFallSpeed);
        }
    }
}
