namespace rwby
{
    public class StateConditionMapper : HnSF.StateConditionMapperBase
    {
        public StateConditionMapper()
        {
            functions.Add(typeof(ConditionNone), BaseStateConditionFunctions.NoCondition);
            functions.Add(typeof(ConditionMoveStickMagnitude), BaseStateConditionFunctions.MovementStickMagnitude);
            functions.Add(typeof(ConditionFallSpeed), BaseStateConditionFunctions.FallSpeed);
            functions.Add(typeof(ConditionIsGrounded), BaseStateConditionFunctions.IsGrounded);
            functions.Add(typeof(ConditionButton), BaseStateConditionFunctions.Button);
            functions.Add(typeof(ConditionButtonSequence), BaseStateConditionFunctions.ButtonSequence);
            functions.Add(typeof(ConditionAnd), BaseStateConditionFunctions.ANDCondition);
            functions.Add(typeof(ConditionOr), BaseStateConditionFunctions.ORCondition);
            functions.Add(typeof(ConditionCanAirJump), BaseStateConditionFunctions.CanAirJump);
            functions.Add(typeof(ConditionCanAirDash), BaseStateConditionFunctions.CanAirDash);
            functions.Add(typeof(ConditionMoveset), BaseStateConditionFunctions.Moveset);
            functions.Add(typeof(ConditionHitstunValue), BaseStateConditionFunctions.HitstunValue);
            functions.Add(typeof(ConditionBlockstunValue), BaseStateConditionFunctions.BlockstunValue);
            functions.Add(typeof(ConditionLockedOn), BaseStateConditionFunctions.LockedOn);
            functions.Add(typeof(ConditionWallValid), BaseStateConditionFunctions.WallValid);
            functions.Add(typeof(ConditionHoldingTowardsWall), BaseStateConditionFunctions.HoldingTowardsWall);
            functions.Add(typeof(ConditionHitboxHitCount), BaseStateConditionFunctions.HitboxHitCount);
            functions.Add(typeof(ConditionHitCount), BaseStateConditionFunctions.HitCount);
            functions.Add(typeof(ConditionFloorAngle), BaseStateConditionFunctions.FloorAngle);
            functions.Add(typeof(ConditionCompareSlopeDir), BaseStateConditionFunctions.CompareSlopeDir);
            functions.Add(typeof(ConditionPoleValid), BaseStateConditionFunctions.PoleValid);
            functions.Add(typeof(ConditionHasThrowees), BaseStateConditionFunctions.HasThrowees);
            functions.Add(typeof(ConditionCompareInputDir), BaseStateConditionFunctions.CompareInputDir);
        }
    }
}