using Fusion;
using HnSF.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [OrderAfter(typeof(Fusion.HitboxManager))]
    public class FighterBoxManager : NetworkBehaviour, IBoxCollection
    {
        [Networked] public int CurrentBoxCollectionIdentifier { get; set; }

        public CustomHitbox[] Hitboxes { get { return hitboxes; } }
        public Hurtbox[] Hurtboxes { get { return hurtboxes; } }
        public Collbox[] Collboxes { get { return collboxes; } }

        [SerializeField] protected FighterCombatManager combatManager;
        public HitboxRoot hRoot;
        public Settings settings;

        public CustomHitbox[] hitboxes;
        public Hurtbox[] hurtboxes;
        public Collbox[] collboxes;
        public Throwablebox[] throwableboxes;

        public void Awake()
        {
            settings = GameManager.singleton.settings;
            foreach (var hitbox in hitboxes)
            {
                hitbox.ownerNetworkObject = combatManager.GetComponent<NetworkObject>();
            }
            foreach (var hb in hurtboxes)
            {
                hb.ownerNetworkObject = combatManager.GetComponent<NetworkObject>();
                hb.hurtable = combatManager;
            }
            foreach(var cb in collboxes)
            {
                cb.ownerNetworkObject = combatManager.GetComponent<NetworkObject>();
            }
            foreach(var tb in throwableboxes)
            {
                tb.ownerNetworkObject = combatManager.GetComponent<NetworkObject>();
            }
        }

        public Bounds combatBoxBounds;

        public override void FixedUpdateNetwork()
        {
            combatBoxBounds = new Bounds(Vector3.zero, -Vector3.one);
        
            foreach(CustomHitbox hb in hitboxes)
            {
                switch (hb.Type)
                {
                    case HitboxTypes.Box:
                        combatBoxBounds.Encapsulate(new Bounds(hb.transform.position+hb.Offset, hb.BoxExtents));
                        break;
                    case HitboxTypes.Sphere:
                        combatBoxBounds.Encapsulate(new Bounds(hb.transform.position+hb.Offset, new Vector3(hb.SphereRadius, hb.SphereRadius, hb.SphereRadius)));
                        break;
                }
            }

            if (combatBoxBounds.size == -Vector3.one) return;
            CombatPairFinder.singleton.RegisterObject(Object);
        }
        
        public void ClearBoxes()
        {
            foreach (Hurtbox hb in hurtboxes)
            {
                hRoot.SetHitboxActive(hb, false);
            }
            foreach(Collbox cb in collboxes)
            {
                hRoot.SetHitboxActive(cb, false);
            }
            foreach (Throwablebox tb in throwableboxes)
            {
                hRoot.SetHitboxActive(tb, false);
            }
        }

        public void ClearHitboxes()
        {
            foreach(CustomHitbox hitbox in hitboxes)
            {
                hRoot.SetHitboxActive(hitbox, false);
            }
        }

        public void UpdateHitbox(int hitboxNum, HitboxGroup hitboxGroup, int hitboxGroupIndex, int boxIndex)
        {
            CustomHitbox hb = hitboxes[hitboxNum];

            switch (hitboxGroup.boxes[boxIndex].shape)
            {
                case HnSF.Combat.BoxShape.Rectangle:
                    hb.Type = HitboxTypes.Box;
                    hb.BoxExtents = (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).size / 2.0f;
                    break;
                case HnSF.Combat.BoxShape.Circle:
                    hb.Type = HitboxTypes.Sphere;
                    hb.SphereRadius = (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).radius;
                    break;
            }
            hb.groupIndex = hitboxGroupIndex;
            hb.hitboxGroup = hitboxGroup;
            hb.hitboxIndex = boxIndex;
            hb.transform.localPosition = (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).offset;
            hb.Root.SetHitboxActive(hb, true);
        }

        public void UpdateBoxes(int frame, int boxCollectionIdentifier)
        {
            CurrentBoxCollectionIdentifier = boxCollectionIdentifier;
            ClearBoxes();

            /*
            if(!ValidateBoxCount(shd.hurtboxCount, hurtboxes.Length, ref hurtboxes) 
                || !ValidateBoxCount(shd.collboxCount, collboxes.Length, ref collboxes) 
                || !ValidateBoxCount(shd.throwableboxCount, throwableboxes.Length, ref throwableboxes))
            {
                return;
            }

            UpdateBoxes(ref shd.hurtboxGroups, ref hurtboxes);
            UpdateBoxes(ref shd.collboxGroups, ref collboxes);
            UpdateBoxes(ref shd.throwableboxGroups, ref throwableboxes);*/
        }

        private bool ValidateBoxCount<T>(int wantedCount, int maxCount, ref T[] boxList) where T : Fusion.Hitbox
        {
            if (wantedCount <= maxCount)
            {
                for (int i = boxList.Length - 1; i >= wantedCount; i--)
                {
                    hRoot.SetHitboxActive(boxList[i], false);
                }
            }
            else if (wantedCount > maxCount)
            {
                Debug.LogError($"More boxes requested than available for {gameObject}");
                return false;
            }
            return true;
        }

        private void UpdateBoxes<T>(ref List<HurtboxGroup> boxGroup, ref T[] boxes) where T : Custombox
        {
            int counter = 0;
            for (int i = 0; i < boxGroup.Count; i++)
            {
                for (int j = 0; j < boxGroup[i].boxes.Count; j++)
                {
                    UpdateBoxes(boxGroup[i], (HnSF.Combat.BoxDefinition)boxGroup[i].boxes[j], boxes[counter]);
                    counter++;
                }
            }
        }

        private void UpdateBoxes(HnSF.Combat.HurtboxGroup hurtboxGroup, HnSF.Combat.BoxDefinition boxDef, Custombox box)
        {
            switch (boxDef.shape)
            {
                case HnSF.Combat.BoxShape.Rectangle:
                    box.Type = HitboxTypes.Box;
                    box.BoxExtents = boxDef.size / 2.0f;
                    break;
                case HnSF.Combat.BoxShape.Circle:
                    box.Type = HitboxTypes.Sphere;
                    box.SphereRadius = boxDef.radius;
                    break;
            }
            box.transform.localPosition = boxDef.offset;
            box.hurtboxGroup = hurtboxGroup;
            box.Root.SetHitboxActive(box, true);
        }
    }
}