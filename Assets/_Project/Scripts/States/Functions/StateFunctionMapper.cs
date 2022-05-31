namespace rwby
{
    public class StateFunctionMapper : HnSF.StateFunctionMapperBase
    {
        public StateFunctionMapper()
        {
            functions.Add(typeof(VarEmpty), BaseStateFunctions.Null);
            functions.Add(typeof(VarChangeState), BaseStateFunctions.ChangeState);
            functions.Add(typeof(VarChangeStateList), BaseStateFunctions.ChangeStateList);
            functions.Add(typeof(VarApplyGravity), BaseStateFunctions.ApplyGravity);
            functions.Add(typeof(VarApplyTraction), BaseStateFunctions.ApplyTraction);
            functions.Add(typeof(VarSetMovement), BaseStateFunctions.SetMovement);
            functions.Add(typeof(VarApplyMovement), BaseStateFunctions.ApplyMovement);
            functions.Add(typeof(VarSetFallSpeed), BaseStateFunctions.SetFallSpeed);
            functions.Add(typeof(VarModifyAirDashCount), BaseStateFunctions.ModifyAirDashCount);
            functions.Add(typeof(VarModifyHitstun), BaseStateFunctions.ModifyHitstun);
            functions.Add(typeof(VarSetECB), BaseStateFunctions.SetECB);
            functions.Add(typeof(VarSnapECB), BaseStateFunctions.SnapECB);
            functions.Add(typeof(VarModifyFrame), BaseStateFunctions.ModifyFrame);
            functions.Add(typeof(VarApplyJumpForce), BaseStateFunctions.ApplyJumpForce);
            functions.Add(typeof(VarExternal), BaseStateFunctions.External);
            functions.Add(typeof(VarModifyRotation), BaseStateFunctions.ModifyRotation);
            functions.Add(typeof(VarRotateTowards), BaseStateFunctions.RotateTowards);
            functions.Add(typeof(VarModifyAnimationSet), BaseStateFunctions.ModifyAnimationSet);
            functions.Add(typeof(VarModifyAnimationFrame), BaseStateFunctions.ModifyAnimationFrame);
            functions.Add(typeof(VarModifyAnimationWeight), BaseStateFunctions.ModifyAnimationWeight);
        }
    }
}
