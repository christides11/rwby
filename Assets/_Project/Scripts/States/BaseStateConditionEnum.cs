namespace rwby
{
    [System.Serializable]
    public enum BaseStateConditionEnum : int
    {
        NONE = 0,
        BOOLEAN,
        MOVEMENT_STICK_MAGNITUDE,
        IS_GROUNDED,
        FALL_SPEED,
        BUTTON,
        BUTTON_SEQUENCE
    }
}