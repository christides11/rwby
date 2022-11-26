namespace rwby
{
    public class FighterBoneReferenceCmn : FighterBoneReferenceBase
    {
        public int value;
        
        public override int GetBone()
        {
            return value;
        }
    }
}