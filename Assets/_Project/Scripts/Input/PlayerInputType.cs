namespace rwby
{
    public enum PlayerInputType
    {
        JUMP,
        BLOCK,
        A,
        B,
        DASH,
        LOCK_ON,
        C,
        EXTRA_1,
        EXTRA_2,
        EXTRA_3,
        EXTRA_4,
        TAUNT,
        ABILITY_1,
        ABILITY_2,
        ABILITY_3,
        ABILITY_4
    }

    [System.Flags]
    public enum PlayerInputTypeFlags
    {
        JUMP = 1 << 0,
        BLOCK = 1 << 1,
        A = 1 << 2,
        B = 1 << 3,
        DASH = 1 << 4,
        LOCK_ON = 1 << 5,
        C = 1 << 6,
        EXTRA_1 = 1 << 7,
        EXTRA_2 = 1 << 8,
        EXTRA_3 = 1 << 9,
        EXTRA_4 = 1 << 10,
        TAUNT = 1 << 11,
        ABILITY_1 = 1 << 12,
        ABILITY_2 = 1 << 13,
        ABILITY_3 = 1 << 14,
        ABILITY_4 = 1 << 15
    }
}