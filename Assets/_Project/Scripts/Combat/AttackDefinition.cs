using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "AttackDefinition", menuName = "rwby/Combat/Attack Definition")]
    public class AttackDefinition : HnSF.Combat.AttackDefinition
    {
        public int firstActableFrame = 100;
    }
}
