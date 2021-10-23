using Fusion;
using HnSF.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class FighterBoxManager : NetworkBehaviour
    {
        public HurtboxCollection BoxCollection { get { return boxCollection; } }
        [Networked] public int CurrentBoxCollectionIdentifier { get; set; }
        [SerializeField] protected FighterCombatManager combatManager;
        [SerializeField] protected HurtboxCollection boxCollection;
        public HitboxRoot hRoot;
        public Settings settings;

        public Hurtbox[] hurtboxes;
        public Collbox[] collboxes;
        public Throwablebox[] throwableboxes;

        public void Awake()
        {
            settings = GameManager.singleton.settings;
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

        public BoxCollectionDefinition GetHurtboxDefinition()
        {
            return BoxCollection.GetHurtbox(CurrentBoxCollectionIdentifier);
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

        public void UpdateBoxes(int frame, int boxCollectionIdentifier)
        {
            CurrentBoxCollectionIdentifier = boxCollectionIdentifier;
            ClearBoxes();
            BoxCollectionDefinition shd = GetHurtboxDefinition();

            if(!ValidateBoxCount(shd.hurtboxCount, hurtboxes.Length, ref hurtboxes) 
                || !ValidateBoxCount(shd.collboxCount, collboxes.Length, ref collboxes) 
                || !ValidateBoxCount(shd.throwableboxCount, throwableboxes.Length, ref throwableboxes))
            {
                return;
            }

            UpdateBoxes(ref shd.hurtboxGroups, ref hurtboxes);
            UpdateBoxes(ref shd.collboxGroups, ref collboxes);
            UpdateBoxes(ref shd.throwableboxGroups, ref throwableboxes);
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
            box.Offset = boxDef.offset;
            box.hurtboxGroup = hurtboxGroup;
            box.Root.SetHitboxActive(box, true);
        }
    }
}