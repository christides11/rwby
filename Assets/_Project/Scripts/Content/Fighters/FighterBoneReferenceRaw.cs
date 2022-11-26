namespace rwby
{
    public class FighterBoneReferenceRaw : FighterBoneReferenceBase
    {
        public int value;
        
        public override int GetBone()
        {
            return value;
        }
    }
}