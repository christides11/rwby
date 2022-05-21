namespace rwby
{
    public class StateFunctionMapper : HnSF.StateFunctionMapperBase
    {
        public StateFunctionMapper()
        {
            functions.Add((int)BaseStateFunctionEnum.NULL, BaseStateFunctions.Null);
            functions.Add((int)BaseStateFunctionEnum.CHANGE_STATE, BaseStateFunctions.ChangeState);
            functions.Add((int)BaseStateFunctionEnum.APPLY_GRAVITY, BaseStateFunctions.ApplyGravity);
            functions.Add((int)BaseStateFunctionEnum.APPLY_TRACTION, BaseStateFunctions.ApplyTraction);
            functions.Add((int)BaseStateFunctionEnum.SET_MOVEMENT, BaseStateFunctions.SetMovement);
            functions.Add((int)BaseStateFunctionEnum.APPLY_MOVEMENT, BaseStateFunctions.ApplyMovement);
            functions.Add((int)BaseStateFunctionEnum.SET_FALL_SPEED, BaseStateFunctions.SetFallSpeed);
            functions.Add((int)BaseStateFunctionEnum.MODIFY_AIR_DASH_COUNT, BaseStateFunctions.ModifyAirDashCount);
            functions.Add((int)BaseStateFunctionEnum.MODIFY_HITSTUN, BaseStateFunctions.ModifyHitstun);
            functions.Add((int)BaseStateFunctionEnum.SET_ECB, BaseStateFunctions.SetECB);
            functions.Add((int)BaseStateFunctionEnum.SNAP_ECB, BaseStateFunctions.SnapECB);
            functions.Add((int)BaseStateFunctionEnum.MODIFY_FRAME, BaseStateFunctions.ModifyFrame);
            functions.Add((int)BaseStateFunctionEnum.APPLY_JUMP_FORCE, BaseStateFunctions.ApplyJumpForce);
            functions.Add((int)BaseStateFunctionEnum.EXTERNAL, BaseStateFunctions.External);
            functions.Add((int)BaseStateFunctionEnum.MODIFY_ROTATION, BaseStateFunctions.ModifyRotation);
            functions.Add((int)BaseStateFunctionEnum.ROTATE_TOWARDS, BaseStateFunctions.RotateTowards);
        }
    }
}
