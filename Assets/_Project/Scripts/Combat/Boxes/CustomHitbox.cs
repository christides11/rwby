using Fusion;
using UnityEngine;

namespace rwby {
    public class CustomHitbox : Custombox
    {
        public IBoxDefinitionCollection definition;
        public int definitionIndex;

        public virtual void SetBoxActiveState(bool state)
        {
            
        }
        
        public virtual void SetBoxSize(Vector3 offset, Vector3 boxExtents)
        {
            Type = HitboxTypes.Box;
            transform.localPosition = offset;
            BoxExtents = boxExtents;
        }

        public virtual void SetSphereSize(Vector3 offset, float sphereRadius)
        {
            Type = HitboxTypes.Sphere;
            transform.localPosition = offset;
            SphereRadius = sphereRadius;
        }
    }
}