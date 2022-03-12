namespace rwby
{
    public interface IBoxDefinitionCollection
    {
        HitInfo[] HitboxInfo { get; }
        ThrowInfo[] ThrowboxInfo { get; }
        HurtboxInfo[] HurtboxInfo { get; }
    }
}