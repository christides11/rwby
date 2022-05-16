namespace rwby
{
    public interface IInputProvider
    {
        NetworkPlayerInputData GetInput(int inputIndex);
    }
}