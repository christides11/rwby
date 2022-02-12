namespace rwby
{
    public interface IBoxCollection
    {
        public CustomHitbox[] Hitboxes { get; }
        public Hurtbox[] Hurtboxes { get; }
        public Collbox[] Collboxes { get; }
    }
}