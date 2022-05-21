namespace rwby
{
    public class StateConditionMapper : HnSF.StateConditionMapperBase
    {
        public StateConditionMapper()
        {
            functions.Add((int)BaseStateConditionEnum.NONE, BaseStateConditionFunctions.NoCondition);
            functions.Add((int)BaseStateConditionEnum.MOVEMENT_STICK_MAGNITUDE, BaseStateConditionFunctions.MovementStickMagnitude);
            functions.Add((int)BaseStateConditionEnum.FALL_SPEED, BaseStateConditionFunctions.FallSpeed);
            functions.Add((int)BaseStateConditionEnum.IS_GROUNDED, BaseStateConditionFunctions.IsGrounded);
            functions.Add((int)BaseStateConditionEnum.BUTTON, BaseStateConditionFunctions.Button);
            functions.Add((int)BaseStateConditionEnum.BUTTON_SEQUENCE, BaseStateConditionFunctions.ButtonSequence);
        }
    }
}