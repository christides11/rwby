using Fusion;
using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rwby
{
    public class FighterHitboxManager : NetworkBehaviour
    {
        protected GameManager gameManager;
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCombatManager combatManager;

        public LayerMask hitLayermask;
        List<LagCompensatedHit> lch = new List<LagCompensatedHit>();

        private void Awake()
        {
            gameManager = GameManager.singleton;
        }

        public virtual void Reset()
        {

        }

        protected List<Hurtbox> hurtboxes = new List<Hurtbox>();
        public virtual bool CheckForCollision(int hitboxGroupIndex, HitboxGroup hitboxGroup, GameObject attacker, List<GameObject> ignoreList = null)
        {
            bool hurtboxHit = false;
            hurtboxes.Clear();
            for (int i = 0; i < hitboxGroup.boxes.Count; i++)
            {
                CheckBoxCollision(hitboxGroup, i);

                // This hitbox hit nothing.
                if (hurtboxes.Count == 0)
                {
                    continue;
                }

                // Hit thing(s). Check if we should actually hurt them.
                /*if (!collidedIHurtables.ContainsKey(hitboxGroup.ID))
                {
                    collidedIHurtables.Add(hitboxGroup.ID, new IDGroupCollisionInfo());
                }*/

                SortHitHurtboxes();

                for (int j = 0; j < hurtboxes.Count; j++)
                {
                    if (ignoreList != null && ignoreList.Contains(hurtboxes[j].Root.gameObject))
                    {
                        continue;
                    }
                    if (TryHitHurtbox(hitboxGroup, i, j, hitboxGroupIndex, attacker))
                    {
                        hurtboxHit = true;
                    }
                }
            }
            return hurtboxHit;
        }

        protected virtual void SortHitHurtboxes()
        {
            hurtboxes = hurtboxes.OrderBy(x => x?.hurtboxGroup.ID).ToList();
        }

        protected virtual bool TryHitHurtbox(HitboxGroup hitboxGroup, int hitboxIndex, int hurtboxIndex, int hitboxGroupIndex, GameObject attacker)
        {
            // Owner was already hit by this ID group or is null, ignore it.
            if(hurtboxes[hurtboxIndex] == null)
            {
                return false;
            }
            /*if (hurtboxes[hurtboxIndex] == null || collidedIHurtables[hitboxGroup.ID].hitIHurtables.Contains(hurtboxes[hurtboxIndex].Owner))
            {
                return false;
            }*/
            // Additional filtering.
            if (ShouldHurt(hitboxGroup, hitboxIndex, hurtboxes[hurtboxIndex]) == false)
            {
                return false;
            }
            //collidedIHurtables[hitboxGroup.ID].hitIHurtables.Add(hurtboxes[hurtboxIndex].Owner);
            //collidedIHurtables[hitboxGroup.ID].hitboxGroups.Add(hitboxGroupIndex);
            HurtHurtbox(hitboxGroup, hitboxIndex, hurtboxes[hurtboxIndex], attacker);
            return true;
        }

        protected virtual void HurtHurtbox(HitboxGroup hitboxGroup, int hitboxIndex, Hurtbox hurtbox, GameObject attacker)
        {
            HitReaction reaction = (HitReaction)hurtbox.hurtable.Hurt(BuildHurtInfo(hitboxGroup, hitboxIndex, hurtbox, attacker));
            switch (reaction.reaction)
            {
                case HitReactionType.HIT:
                    combatManager.SetHitStop((hitboxGroup.hitboxHitInfo as HitInfo).attackerHitstop);
                    break;
            }
        }

        protected virtual HurtInfoBase BuildHurtInfo(HitboxGroup hitboxGroup, int hitboxIndex, Hurtbox hurtbox, GameObject attacker)
        {
            HurtInfo hurtInfo;

            switch (hitboxGroup.hitboxHitInfo.forceRelation)
            {
                case HitboxForceRelation.ATTACKER:
                    hurtInfo = new HurtInfo((HitInfo)hitboxGroup.hitboxHitInfo, hurtbox.hurtboxGroup as HurtboxGroup,
                        transform.position, manager.transform.forward, manager.transform.right,
                        manager.PhysicsManager.GetOverallForce());
                    break;
                /*case HitboxForceRelation.HITBOX:
                    Vector3 position = hitboxGroup.attachToEntity ? manager.transform.position + (hitboxGroup.boxes[hitboxIndex] as HnSF.Combat.BoxDefinition).offset
                : referencePosition + (hitboxGroup.boxes[hitboxIndex] as HnSF.Combat.BoxDefinition).offset;
                    hurtInfo = new HurtInfo((Combat.HitInfo)hitboxGroup.hitboxHitInfo, hurtbox.HurtboxGroup as Mahou.Combat.HurtboxGroup,
                        position, manager.visual.transform.forward, manager.visual.transform.right,
                        (manager.PhysicsManager as FighterPhysicsManager).GetOverallForce());
                    break;*/
                case HitboxForceRelation.WORLD:
                    hurtInfo = new HurtInfo((HitInfo)hitboxGroup.hitboxHitInfo, hurtbox.hurtboxGroup as HurtboxGroup,
                        transform.position, Vector3.forward, Vector3.right,
                        (manager.PhysicsManager as FighterPhysicsManager).GetOverallForce());
                    break;
                default:
                    hurtInfo = new HurtInfo((HitInfo)hitboxGroup.hitboxHitInfo, hurtbox.hurtboxGroup as HurtboxGroup,
                        transform.position, manager.transform.forward, manager.transform.right,
                        manager.PhysicsManager.GetOverallForce());
                    break;
            }
            return hurtInfo;
        }

        /// <summary>
        /// Determines if this hurtbox should be hit.
        /// </summary>
        /// <param name="hitboxGroup">The hitbox group of the hitbox.</param>
        /// <param name="hitboxIndex">The index of the hitbox.</param>
        /// <param name="hurtbox">The hurtbox that was hit.</param>
        /// <returns>If the hurtbox should be hurt.</returns>
        protected virtual bool ShouldHurt(HitboxGroup hitboxGroup, int hitboxIndex, Hurtbox hurtbox)
        {
            return true;
        }

        protected virtual void CheckBoxCollision(HitboxGroup hitboxGroup, int boxIndex)
        {
            Vector3 modifiedOffset = (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).offset;
            modifiedOffset = modifiedOffset.x * transform.right
                + modifiedOffset.z * transform.forward
                + modifiedOffset.y * Vector3.up;
            Vector3 position = transform.position + modifiedOffset;
            Vector3 size = (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).size;

            int cldAmt = 0;
            switch (hitboxGroup.boxes[boxIndex].shape)
            {
                case BoxShape.Rectangle:
                    //cldAmt = Runner.GetPhysicsScene().OverlapBox(position, size / 2.0f, raycastHitList,
                    //    Quaternion.Euler((hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).rotation),
                    //    hitLayermask);
                    if (gameManager.settings.showHitboxes)
                    {
                        ExtDebug.DrawBox(position, size, Quaternion.Euler((hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).rotation), Color.red, Runner.Simulation.DeltaTime);
                    }
                    break;
                case BoxShape.Circle:
                    cldAmt = Runner.LagCompensation.OverlapSphere(position, (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).radius, Runner.Simulation.Tick, lch, hitLayermask,
                        HitOptions.SubtickAccuracy);
                    if (gameManager.settings.showHitboxes)
                    {
                        ExtDebug.DrawWireSphere(position, (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).radius, Color.red, Runner.Simulation.DeltaTime, 3);
                    }
                    break;
            }

            if (hurtboxes.Count < lch.Count)
            {
                hurtboxes.AddRange(new Hurtbox[lch.Count - hurtboxes.Count]);
            }

            for (int i = 0; i < cldAmt; i++)
            {
                Hurtbox h = lch[i].Object.GetComponent<Hurtbox>();
                if (h.HitboxActive == true && h.Root.gameObject != gameObject)
                {
                    hurtboxes[i] = h;
                }
            }
        }
    }
}
