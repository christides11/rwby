namespace rwby
{
    [System.Serializable]
    public enum BaseStateFunctionEnum : int
    {
        NULL = 0,
        CHANGE_STATE,
        APPLY_GRAVITY,
        APPLY_TRACTION,
        SET_FALL_SPEED,
        SET_MOVEMENT,
        APPLY_MOVEMENT,
        ADD_MOVEMENT,
        MODIFY_AIR_DASH_COUNT,
        MODIFY_HITSTUN,
        SET_ECB,
        SNAP_ECB,
        MODIFY_FRAME,
        APPLY_JUMP_FORCE,
        EXTERNAL,
        MODIFY_ROTATION,
        ROTATE_TOWARDS
    }
}