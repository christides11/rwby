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
        ATTACK = 10,
        FLINCH_GROUND = 11,
        FLINCH_AIR = 12,
        TUMBLE = 13,
        GROUND_BOUNCE = 14,
        BLOCK_HIGH = 15,
        BLOCK_LOW = 16,
        BLOCK_AIR = 17,
        WALL_RUN_V = 18,
        WALL_RUN_H = 19,
        WALL_JUMP = 20,
        TRIP = 21,
        GROUND_LAY_FACE_UP = 22,
        TECH_AIR = 23,
        TECH_GROUND = 24,
        GROUND_GETUP = 25,
        JUMP_END = 26,
        RUN_INIT = 27,
        LANDING = 28,
        GROUND_LAY_FACE_DOWN,
        HIT_GROUND_UPPER, //Hit in the upper body on the ground.
        HIT_GROUND_LOWER, //Hit in the lower body on the ground.
        HIT_AERIAL, // Hit while aerial. Will transition to hit_ground_lower if you land.
        HIT_AERIAL_REELING, // Reeling back, with hands and feet lagging forward. Commonly used for horizontal launching moves.
        HIT_AERIAL_FACE_UP, // Ends with character facing upwards. Commonly used for most aerial hits.
        HIT_AERIAL_FACE_DOWN, // Ends with character facing downwards. Commonly used for tripping moves.
        HIT_AERIAL_LAUNCH, // Ends with character facing downwards. Commonly used for DPs.
        HIT_AERIAL_SOMERSAULT, // Character spins. 
        STAGGER,
        CRUMPLE
    }
}