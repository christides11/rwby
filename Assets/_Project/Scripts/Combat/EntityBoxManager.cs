using Fusion;
using HnSF.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [OrderAfter(typeof(Fusion.HitboxManager))]
    public class EntityBoxManager : NetworkBehaviour, IBoxCollection
    {
        public CustomHitbox[] Hitboxes { get { return hitboxes; } }
        public Hurtbox[] Hurtboxes { get { return hurtboxes; } }
        public Collbox[] Collboxes { get { return collisionboxes; } }
        
        public NetworkObject networkObject;
        public IHurtable hurtable;
        public HitboxRoot hRoot;
        public Settings settings;

        public CustomHitbox[] hitboxes;
        public Hurtbox[] hurtboxes;
        [FormerlySerializedAs("collboxes")] public Collbox[] collisionboxes;
        public Throwablebox[] throwableboxes;
        public CustomHitbox[] throwboxes;

        public virtual void Awake()
        {
            settings = GameManager.singleton.settings;
            foreach (var hitbox in hitboxes)
            {
                hitbox.ownerNetworkObject = networkObject;
            }
            foreach (var hb in hurtboxes)
            {
                hb.ownerNetworkObject = networkObject;
                hb.hurtable = hurtable;
            }
            foreach(var cb in collisionboxes)
            {
                cb.ownerNetworkObject = networkObject;
            }
            foreach(var tb in throwableboxes)
            {
                tb.ownerNetworkObject = networkObject;
            }
        }

        public Bounds combatBoxBounds;

        public override void FixedUpdateNetwork()
        {
            /*
            combatBoxBounds = new Bounds(Vector3.zero, -Vector3.one);
        
            foreach(CustomHitbox hb in hitboxes)
            {
                switch (hb.Type)
                {
                    case HitboxTypes.Box:
                        combatBoxBounds.Encapsulate(new Bounds(hb.transform.position, hb.BoxExtents));
                        break;
                    case HitboxTypes.Sphere:
                        combatBoxBounds.Encapsulate(new Bounds(hb.transform.position, new Vector3(hb.SphereRadius, hb.SphereRadius, hb.SphereRadius)));
                        break;
                }
            }

            if (combatBoxBounds.size == -Vector3.one) return;*/
            CombatPairFinder.singleton.RegisterObject(Object);
        }

        public void ResetAllBoxes()
        {
            foreach (Hurtbox hb in hurtboxes)
            {
                hRoot.SetHitboxActive(hb, false);
                hb.SetBoxActiveState(false);
            }
            foreach (var hb in hitboxes)
            {
                hRoot.SetHitboxActive(hb, false);
                hb.SetBoxActiveState(false);
            }
            foreach(Collbox cb in collisionboxes)
            {
                hRoot.SetHitboxActive(cb, false);
                cb.SetBoxActiveState(false);
            }
            foreach (Throwablebox tb in throwableboxes)
            {
                hRoot.SetHitboxActive(tb, false);
                tb.SetBoxActiveState(false);
            }
        }

        public void AddBox(FighterBoxType boxType, int attachedTo, BoxShape shape, Vector3 offset, Vector3 boxExtents, float sphereRadius, 
            int definitionIndex, IBoxDefinitionCollection definition)
        {
            CustomHitbox fusionHitbox = GetNextCustomHitbox(boxType);
            SetFusionHitboxSize(fusionHitbox, shape, offset, boxExtents, sphereRadius);
            fusionHitbox.definition = definition;
            fusionHitbox.definitionIndex = definitionIndex;
            hRoot.SetHitboxActive(fusionHitbox, true);
            fusionHitbox.SetBoxActiveState(true);
        }

        private CustomHitbox GetNextCustomHitbox(FighterBoxType boxType)
        {
            switch (boxType)
            {
                case FighterBoxType.Hurtbox:
                    for (int i = 0; i < hurtboxes.Length; i++)
                    {
                        if (hurtboxes[i].HitboxActive == false) return hurtboxes[i];
                    }
                    break;
                case FighterBoxType.Hitbox:
                    for (int i = 0; i < hitboxes.Length; i++)
                    {
                        if (hitboxes[i].HitboxActive == false) return hitboxes[i];
                    }
                    break;
                case FighterBoxType.Collisionbox:
                    for (int i = 0; i < collisionboxes.Length; i++)
                    {
                        if (collisionboxes[i].HitboxActive == false) return collisionboxes[i];
                    }
                    break;
                case FighterBoxType.Throwablebox:
                    for (int i = 0; i < throwableboxes.Length; i++)
                    {
                        if (throwableboxes[i].HitboxActive == false) return throwableboxes[i];
                    }
                    break;
            }
            return null;
        }

        private void SetFusionHitboxSize(CustomHitbox fusionHitbox, BoxShape shape, Vector3 offset, Vector3 boxExtents, float sphereRadius)
        {
            //fusionHitbox.transform.localPosition = offset;
            switch (shape)
            {
                case BoxShape.Rectangle:
                    fusionHitbox.SetBoxSize(offset, boxExtents);
                    break;
                case BoxShape.Circle:
                    fusionHitbox.SetSphereSize(offset, sphereRadius);
                    break;
            }
        }
    }
}