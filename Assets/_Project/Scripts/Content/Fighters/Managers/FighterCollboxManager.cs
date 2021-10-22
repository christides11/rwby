using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class FighterCollboxManager : NetworkBehaviour
    {
        public HurtboxCollection CollboxCollection { get { return collboxCollection; } }

        [SerializeField] protected FighterCombatManager combatManager;
        [SerializeField] protected HurtboxCollection collboxCollection;
        public HitboxRoot hRoot;

        public Settings settings;

        [Networked] public int CurrentCollboxIdentifier { get; set; }
        [Networked] public int CollboxCount { get; set; }

        public Collbox[] collisionboxes;

        public void Awake()
        {
            settings = GameManager.singleton.settings;
            foreach (var hb in collisionboxes)
            {
                //hb.hurtableNetworkObject = combatManager.GetComponent<NetworkObject>();
                //hb.hurtable = combatManager;
            }
        }

        public void CreateCollboxes(int frame, int hurtboxIdentifier)
        {
            CurrentCollboxIdentifier = hurtboxIdentifier;
            ResetCollboxes();
            StateHurtboxDefinition shd = GetHurtboxDefinition();

            CollboxCount = GetCollboxCount(shd);
            CheckCollboxCount(CollboxCount);
            UpdateCollboxes(shd);
        }

        public StateHurtboxDefinition GetHurtboxDefinition()
        {
            return CollboxCollection.GetCollbox(CurrentCollboxIdentifier);
        }

        public void ResetCollboxes()
        {
            foreach (Collbox hb in collisionboxes)
            {
                hRoot.SetHitboxActive(hb, false);
            }
        }

        private void CheckCollboxCount(int hurtboxCount)
        {
            if (hurtboxCount < collisionboxes.Length)
            {
                for (int i = collisionboxes.Length - 1; i >= hurtboxCount; i--)
                {
                    hRoot.SetHitboxActive(collisionboxes[i], false);
                }
            }
            else if (hurtboxCount >= collisionboxes.Length)
            {
                Debug.LogError($"More collboxes requested than available for {gameObject}");
            }
        }

        private int GetCollboxCount(StateHurtboxDefinition shd)
        {
            int counter = 0;
            for (int i = 0; i < shd.hurtboxGroups.Count; i++)
            {
                counter += shd.hurtboxGroups[i].boxes.Count;
            }
            return counter;
        }

        private void UpdateCollboxes(StateHurtboxDefinition shd)
        {
            int counter = 0;
            for (int i = 0; i < shd.hurtboxGroups.Count; i++)
            {
                for (int j = 0; j < shd.hurtboxGroups[i].boxes.Count; j++)
                {
                    UpdateCollbox(shd.hurtboxGroups[i], (HnSF.Combat.BoxDefinition)shd.hurtboxGroups[i].boxes[j], (Hurtbox)hRoot.Hitboxes[counter]);
                    counter++;
                }
            }
        }

        private void UpdateCollbox(HnSF.Combat.HurtboxGroup hurtboxGroup, HnSF.Combat.BoxDefinition boxDef, Hurtbox hitbox)
        {
            switch (boxDef.shape)
            {
                case HnSF.Combat.BoxShape.Rectangle:
                    hitbox.Type = HitboxTypes.Box;
                    hitbox.BoxExtents = boxDef.size / 2.0f;
                    break;
                case HnSF.Combat.BoxShape.Circle:
                    hitbox.Type = HitboxTypes.Sphere;
                    hitbox.SphereRadius = boxDef.radius;
                    break;
            }
            hitbox.Offset = boxDef.offset;
            hitbox.hurtboxGroup = hurtboxGroup;
            hitbox.Root.SetHitboxActive(hitbox, true);
        }
    }
}