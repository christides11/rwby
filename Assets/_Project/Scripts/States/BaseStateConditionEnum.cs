namespace rwby
{
    [System.Serializable]
    public enum BaseStateConditionEnum : int
    {
        NONE = 0,
        BOOLEAN,
        MOVEMENT_MAGNITUDE,
        GROUNDED_STATE,
        FALL_SPEED,
        BUTTON
    }
}