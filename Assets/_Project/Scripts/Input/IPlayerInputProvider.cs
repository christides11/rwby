namespace rwby
{
    public interface IPlayerInputProvider
    {
        NetworkPlayerInputData GetInput(int playerIndex);
    }
}