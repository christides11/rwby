namespace rwby
{
    [System.Serializable]
    public enum FighterCmnStates : ushort
    {
        NULL = 0,
        IDLE = 1,
        WALK = 2,
        RUN = 3,
        JUMP = 4,
        FALL = 5,
        JUMPSQUAT = 6,
        RUN_BRAKE = 7,
        AIR_JUMP = 8,
        AIR_DASH = 9,
        STAGGER = 10,
        CRUMPLE = 11,
        GROUND_BOUNCE = 14,
        GUARD = 15,
        BLOCK_LOW = 16,
        GUARD_AIR = 17,
        WALL_RUN_V = 18,
        WALL_RUN_H = 19,
        WALL_JUMP = 20,
        GROUND_TECH_ROLL = 21,
        GROUND_LAY_FACE_UP = 22,
        TECH_AIR = 23,
        GROUND_TECH_NEUTRAL = 24,
        GROUND_TECH_RISE = 25,
        JUMP_END = 26,
        RUN_INIT = 27,
        LANDING = 28,
        GROUND_LAY_FACE_DOWN = 29,
        HIT_GROUND_UPPER = 30, //Hit in the upper body on the ground.
        HIT_GROUND_LOWER = 31, //Hit in the lower body on the ground.
        HIT_AERIAL = 32, // Hit while aerial. Will transition to hit_ground_lower if you land.
        HIT_AERIAL_REELING = 33, // Reeling back, with hands and feet lagging forward. Commonly used for horizontal launching moves.
        HIT_AERIAL_FACE_UP = 34, // Ends with character facing upwards. Commonly used for most aerial hits.
        HIT_AERIAL_FACE_DOWN = 35, // Ends with character facing downwards. Commonly used for tripping moves.
        HIT_AERIAL_LAUNCH = 36, // Ends with character facing downwards. Commonly used for DPs.
        HIT_AERIAL_SOMERSAULT = 37, // Character spins. 
        WALL_CLING,
        RUN_SLIDE,
        POLE_SPIN,
        POLE_LAUNCH,
        THROWN,
        THROW_ATTEMPT,
        THROW,
        SHIELD_GRD,
        SHIELD_AIR,
        WALL_BOUNCE,
        JUMPSQUAT_MOMENTUM,
        DEATH,
        THROW_TECH = 12,
        THROW_TECH_AIR = 13,
        TAUNT = 50,
        TAUNT_AIR,
        BURST,
        GUARD_COUNTER
    }
}