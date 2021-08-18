namespace rwby
{
    public interface StatBase<T>
    {
        T BaseValue { get; }

        public void UpdateBaseValue(T value);
        public T GetCurrentValue();
        public void Dirty();
    }
}