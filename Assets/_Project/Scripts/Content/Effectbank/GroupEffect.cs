namespace rwby
{
    public class GroupEffect : BaseEffect
    {
        public BaseEffect[] childEffects;

        public override void SetFrame(float time)
        {
            base.SetFrame(time);
            for (int i = 0; i < childEffects.Length; i++)
            {
                childEffects[i].SetFrame(time);
            }
        }
    }
}