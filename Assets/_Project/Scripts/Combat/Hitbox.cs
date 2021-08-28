using UnityEngine;
using Fusion;

namespace rwby
{
    public class Hitbox : MonoBehaviour
    {
        public Fusion.Hitbox fusionHitbox;

        public void SetBox(Vector3 boxExtends)
        {
            fusionHitbox.Type = Fusion.HitboxTypes.Box;
            fusionHitbox.BoxExtents = boxExtends;
        }

        public void SetSphere(float sphereRadius)
        {
            fusionHitbox.Type = HitboxTypes.Sphere;
            fusionHitbox.SphereRadius = sphereRadius;
        }
    }
}