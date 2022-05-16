namespace rwby
{
    [System.Serializable]
    public enum BaseStateFunctionEnum : int
    {
        NULL = 0,
        CHANGE_STATE,
        APPLY_GRAVITY,
        APPLY_TRACTION,
        SET_FALL_SPEED
    }
}