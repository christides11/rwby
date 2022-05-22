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
        }
    }
}