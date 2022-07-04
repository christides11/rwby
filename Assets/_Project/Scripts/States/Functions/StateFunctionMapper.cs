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
            functions.Add(typeof(VarCreateBox), BaseStateFunctions.CreateBox);
            functions.Add(typeof(VarMultiplyMovement), BaseStateFunctions.MultiplyMovement);
            functions.Add(typeof(VarAddMovement), BaseStateFunctions.AddMovement);
            functions.Add(typeof(VarClampGravity), BaseStateFunctions.ClampGravity);
            functions.Add(typeof(VarMultiplyGravity), BaseStateFunctions.MultiplyGravity);
            functions.Add(typeof(VarModifyFallSpeed), BaseStateFunctions.ModifyFallSpeed);
            functions.Add(typeof(VarModifyEffectSet), BaseStateFunctions.ModifyEffectSet);
            functions.Add(typeof(VarModifyEffectFrame), BaseStateFunctions.ModifyEffectFrame);
            functions.Add(typeof(VarModifyEffectRotation), BaseStateFunctions.ModifyEffectRotation);
            functions.Add(typeof(VarTrySpecial), BaseStateFunctions.TrySpecial);
            functions.Add(typeof(VarCreateProjectile), BaseStateFunctions.CreateProjectile);
            functions.Add(typeof(VarClearHitList), BaseStateFunctions.ClearHitList);
        }
    }
}
