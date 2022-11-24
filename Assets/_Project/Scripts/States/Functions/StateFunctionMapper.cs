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
            functions.Add(typeof(VarModifyJumpCount), BaseStateFunctions.ModifyJumpCount);
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
            functions.Add(typeof(VarProjectilePointToTarget), BaseStateFunctions.ProjectilePointToTarget);
            functions.Add(typeof(VarClearHitList), BaseStateFunctions.ClearHitList);
            functions.Add(typeof(VarFindSoftTarget), BaseStateFunctions.FindSoftTarget);
            functions.Add(typeof(VarDebugLog), BaseStateFunctions.LogMessage);
            functions.Add(typeof(VarFindWall), BaseStateFunctions.FindWall);
            functions.Add(typeof(VarFindPole), BaseStateFunctions.FindPole);
            functions.Add(typeof(VarSnapToWall), BaseStateFunctions.SnapToWall);
            functions.Add(typeof(VarSnapToPole), BaseStateFunctions.SnapToPole);
            functions.Add(typeof(VarClampMovement), BaseStateFunctions.ClampMovement);
            functions.Add(typeof(VarTeleportRaycast), BaseStateFunctions.TeleportRaycast);
            functions.Add(typeof(VarModifyMoveset), BaseStateFunctions.ModifyMoveset);
            functions.Add(typeof(VarModifyPoleAngle), BaseStateFunctions.ModifyPoleAngle);
            functions.Add(typeof(VarTransferPoleMomentum), BaseStateFunctions.TransferPoleMomentum);
            functions.Add(typeof(VarClearThrowee), BaseStateFunctions.ClearThrowee);
            functions.Add(typeof(VarModifyBlockstun), BaseStateFunctions.ModifyBlockstun);
            functions.Add(typeof(VarSetGuardState), BaseStateFunctions.SetGuardState);
            functions.Add(typeof(VarModifyAura), BaseStateFunctions.ModifyAura);
            functions.Add(typeof(VarSetBlockState), BaseStateFunctions.SetBlockState);
            functions.Add(typeof(VarClearCurrentEffects), BaseStateFunctions.ClearCurrentEffects);
            functions.Add(typeof(VarModifySoundSet), BaseStateFunctions.ModifySoundSet);
            functions.Add(typeof(VarIncrementChargeLevel), BaseStateFunctions.IncrementChargeLevel);
            functions.Add(typeof(VarModifyCameraMode), BaseStateFunctions.ModifyCameraMode);
            functions.Add(typeof(VarModifyAttackStringList), BaseStateFunctions.ModifyAttackStringList);
            functions.Add(typeof(VarSetCounterhitState), BaseStateFunctions.SetCounterhitState);
            functions.Add(typeof(VarSetPushblockState), BaseStateFunctions.SetPushblockState);
            functions.Add(typeof(VarConsumeGroundBounce), BaseStateFunctions.ConsumeGroundBounce);
            functions.Add(typeof(VarConsumeWallBounce), BaseStateFunctions.ConsumeWallBounce);
            functions.Add(typeof(VarMoveTowardsMagnitude), BaseStateFunctions.MoveTowardsMagnitude);
            functions.Add(typeof(VarSetGroundedState), BaseStateFunctions.SetGroundedState);
            functions.Add(typeof(VarModifyIntWhiteboard), BaseStateFunctions.ModifyIntWhiteboard);
            functions.Add(typeof(VarProjectileModifyForce), BaseStateFunctions.ProjectileModifyForce);
            functions.Add(typeof(VarSetProjectileTarget), BaseStateFunctions.SetProjectileTarget);
            functions.Add(typeof(VarProjectileModifyHomingStrength), BaseStateFunctions.ModifyProjectileHomingStrength);
            functions.Add(typeof(VarDirectDamage), BaseStateFunctions.DirectDamage);
        }
    }
}
