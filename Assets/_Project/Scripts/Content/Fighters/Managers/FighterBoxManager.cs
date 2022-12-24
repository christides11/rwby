using Fusion;
using HnSF.Combat;

namespace rwby
{
    [OrderAfter(typeof(Fusion.HitboxManager))]
    public class FighterBoxManager : EntityBoxManager
    {
        public override void Awake()
        {
            hurtable = GetComponent<IHurtable>();
            base.Awake();
        }
    }
}