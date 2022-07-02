namespace rwby
{
    public interface IAttacker
    {
        public bool IsHitHurtboxValid(CustomHitbox atackerHitbox, Hurtbox h);
        public bool IsHitHitboxValid(CustomHitbox attackerHitbox, CustomHitbox h);
        public HurtInfo BuildHurtInfo(CustomHitbox hitbox, Hurtbox enemyHurtbox);
        public void DoHit(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo);
        public void DoClash(CustomHitbox hitbox, CustomHitbox enemyHitbox);
    }
}