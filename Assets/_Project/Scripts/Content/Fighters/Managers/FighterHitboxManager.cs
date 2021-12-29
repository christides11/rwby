using Fusion;
using HnSF.Combat;
using System;
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
        public LayerMask grabLayermask;
        public List<LagCompensatedHit> lch = new List<LagCompensatedHit>();

        [Networked, Capacity(10)] public NetworkArray<IDGroupCollisionInfo> nd { get; }
        [Networked] public byte ndCount { get; set; }

        private void Awake()
        {
            gameManager = GameManager.singleton;
        }

        public virtual void Reset()
        {
            ndCount = 0;
        }

        protected List<Hurtbox> hurtboxes = new List<Hurtbox>();
        #region Hitbox
        public virtual bool CheckHit(int hitboxGroupIndex, HitboxGroup hitboxGroup, GameObject attacker, List<GameObject> ignoreList = null)
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
            if(hurtboxes[hurtboxIndex] == null || CheckHitHurtable(hitboxGroup.ID, hurtboxes[hurtboxIndex]) == true)
            {
                return false;
            }
            // Additional filtering.
            if (ShouldHurt(hitboxGroup, hitboxIndex, hurtboxes[hurtboxIndex]) == false)
            {
                return false;
            }
            AddCollidedHurtable(hitboxGroup.ID, hurtboxes[hurtboxIndex]);
            HurtHurtbox(hitboxGroup, hitboxIndex, hurtboxes[hurtboxIndex], attacker, lch[hurtboxIndex].Point);
            return true;
        }

        private void AddCollidedHurtable(int iD, Hurtbox hurtbox)
        {
            if(ndCount == nd.Length)
            {
                Debug.LogError("Can't add any more collided hurtable information.");
                return;
            }
            nd.Set(ndCount, new IDGroupCollisionInfo() { hitByIDGroup = iD, hitIHurtableNetID = hurtbox.ownerNetworkObject.Id });
            ndCount++;
        }

        private bool CheckHitHurtable(int hitboxIDGroup, Hurtbox hurtbox)
        {
            for(int i = 0; i < ndCount; i++)
            {
               NetworkObject nb = Runner.FindObject(nd[i].hitIHurtableNetID);

                if (nd[i].hitByIDGroup == hitboxIDGroup && nb.GetComponent<IHurtable>() == hurtbox.hurtable)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual void HurtHurtbox(HitboxGroup hitboxGroup, int hitboxIndex, Hurtbox hurtbox, GameObject attacker, Vector3 hitPoint)
        {
            HitReaction reaction = (HitReaction)hurtbox.hurtable.Hurt(BuildHurtInfo(hitboxGroup, hitboxIndex, hurtbox, attacker, hitPoint));
            HitInfo hi = (hitboxGroup.hitboxHitInfo as HitInfo);

            switch (reaction.reaction)
            {
                case HitReactionType.HIT:
                    combatManager.SetHitStop((hitboxGroup.hitboxHitInfo as HitInfo).attackerHitstop);
                    if (string.IsNullOrEmpty(hi.effectbankName) == false) {
                        BaseEffect be = manager.EffectbankContainer.CreateEffect(hurtbox.transform.position + Vector3.up + (-transform.forward.normalized * 0.25f), 
                            attacker.transform.rotation, hi.effectbankName, hi.effectName);
                        be.PlayEffect();
                    }
                    if(string.IsNullOrEmpty(hi.hitSoundbankName) == false)
                    {
                        manager.SoundbankContainer.PlaySound(hi.hitSoundbankName, hi.hitSoundName);
                    }
                    if (Runner.IsResimulation == false && Object.HasInputAuthority == true)
                    {
                        PlayerCamera.instance.ShakeCamera((hitboxGroup.hitboxHitInfo as HitInfo).shakeValue, (hitboxGroup.hitboxHitInfo as HitInfo).hitstop * Runner.DeltaTime);
                    }
                    break;
                case HitReactionType.BLOCKED:
                    combatManager.SetHitStop((hitboxGroup.hitboxHitInfo as HitInfo).blockHitstopAttacker);
                    BaseEffect bb = manager.EffectbankContainer.CreateEffect(hurtbox.transform.position + Vector3.up + (-transform.forward.normalized * 0.5f),
                            attacker.transform.rotation * Quaternion.Euler(0, 180, 0), "global", "shieldhit1");
                    bb.PlayEffect(true, false);
                    if (string.IsNullOrEmpty(hi.blockSoundbankName) == false)
                    {
                        manager.SoundbankContainer.PlaySound(hi.blockSoundbankName, hi.blockSoundName);
                    }
                    break;
            }
        }

        protected virtual HurtInfoBase BuildHurtInfo(HitboxGroup hitboxGroup, int hitboxIndex, Hurtbox hurtbox, GameObject attacker, Vector3 hitPoint)
        {
            HurtInfo hurtInfo;

            switch (hitboxGroup.hitboxHitInfo.forceRelation)
            {
                case HitboxForceRelation.ATTACKER:
                    hurtInfo = new HurtInfo((HitInfo)hitboxGroup.hitboxHitInfo, hurtbox.hurtboxGroup as HurtboxGroup,
                        transform.position, manager.transform.forward, manager.transform.right,
                        manager.PhysicsManager.GetOverallForce(), hitPoint);
                    break;
                case HitboxForceRelation.HITBOX: 
                   Vector3 position = hitboxGroup.attachToEntity ? manager.transform.position + (hitboxGroup.boxes[hitboxIndex] as HnSF.Combat.BoxDefinition).offset
                        : Vector3.zero + (hitboxGroup.boxes[hitboxIndex] as HnSF.Combat.BoxDefinition).offset;
                   hurtInfo = new HurtInfo((HitInfo)hitboxGroup.hitboxHitInfo, hurtbox.hurtboxGroup as HurtboxGroup,
                        position, manager.transform.forward, manager.transform.right,
                        manager.PhysicsManager.GetOverallForce(), hitPoint);
                    break;
                case HitboxForceRelation.WORLD:
                    hurtInfo = new HurtInfo((HitInfo)hitboxGroup.hitboxHitInfo, hurtbox.hurtboxGroup as HurtboxGroup,
                        transform.position, Vector3.forward, Vector3.right,
                        (manager.PhysicsManager as FighterPhysicsManager).GetOverallForce(), hitPoint);
                    break;
                default:
                    hurtInfo = new HurtInfo((HitInfo)hitboxGroup.hitboxHitInfo, hurtbox.hurtboxGroup as HurtboxGroup,
                        transform.position, manager.transform.forward, manager.transform.right,
                        manager.PhysicsManager.GetOverallForce(), hitPoint);
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
                case BoxShape.Circle:
                    cldAmt = Runner.LagCompensation.OverlapSphere(position, (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).radius, Runner.Simulation.Tick, lch, hitLayermask, HitOptions.DetailedHit);
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
                Hurtbox h = lch[i].GameObject.GetComponent<Hurtbox>();
                if (h.HitboxActive == true && h.Root.gameObject != gameObject)
                {
                    hurtboxes[i] = h;
                }
            }
        }
        #endregion

        protected List<Throwablebox> hitThrowableboxes = new List<Throwablebox>();
        #region Grabbox
        public virtual bool CheckGrab(int hitboxGroupIndex, HitboxGroup hitboxGroup, GameObject attacker, List<GameObject> ignoreList = null)
        {
            bool hurtboxHit = false;
            hurtboxes.Clear();
            for (int i = 0; i < hitboxGroup.boxes.Count; i++)
            {
                //CheckBoxCollision(hitboxGroup, i);
                CheckForThrowableboxes(hitboxGroup, i, ref hitThrowableboxes);

                /*
                // This hitbox hit nothing.
                if (hurtboxes.Count == 0)
                {
                    continue;
                }

                SortHitHurtboxes();

                for (int j = 0; j < hurtboxes.Count; j++)
                {
                    if (ignoreList != null && ignoreList.Contains(hurtboxes[j].Root.gameObject))
                    {
                        continue;
                    }
                    if (TryGrab(hitboxGroup, i, j, hitboxGroupIndex, attacker))
                    {
                        hurtboxHit = true;
                    }
                }*/
            }
            return hurtboxHit;
        }

        protected virtual void CheckForThrowableboxes<T>(HitboxGroup boxGroup, int boxIndex, ref List<T> grabbedBoxList ) where T : Custombox
        {
            Vector3 modifiedOffset = (boxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).offset;
            modifiedOffset = modifiedOffset.x * transform.right
                + modifiedOffset.z * transform.forward
                + modifiedOffset.y * Vector3.up;
            Vector3 position = transform.position + modifiedOffset;
            Vector3 size = (boxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).size;

            
            int cldAmt = 0;
            switch (boxGroup.boxes[boxIndex].shape)
            {
                case BoxShape.Circle:
                    cldAmt = Runner.LagCompensation.OverlapSphere(position, (boxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).radius, Runner.Simulation.Tick, lch, hitLayermask, HitOptions.DetailedHit);
                    if (gameManager.settings.showHitboxes)
                    {
                        ExtDebug.DrawWireSphere(position, (boxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).radius, Color.red, Runner.Simulation.DeltaTime, 3);
                    }
                    break;
            }

            
            if (hurtboxes.Count < lch.Count)
            {
                hurtboxes.AddRange(new Hurtbox[lch.Count - hurtboxes.Count]);
            }
            /*
            for (int i = 0; i < cldAmt; i++)
            {
                Hurtbox h = lch[i].GameObject.GetComponent<Hurtbox>();
                if (h.HitboxActive == true && h.Root.gameObject != gameObject)
                {
                    hurtboxes[i] = h;
                }
            }*/
        }

        /*
        protected virtual bool TryGrab(HitboxGroup hitboxGroup, int hitboxIndex, int hurtboxIndex, int hitboxGroupIndex, GameObject attacker)
        {
            // Owner was already hit by this ID group or is null, ignore it.
            if (hurtboxes[hurtboxIndex] == null || CheckHitHurtable(hitboxGroup.ID, hurtboxes[hurtboxIndex]) == true)
            {
                return false;
            }
            // Additional filtering.
            if (ShouldHurt(hitboxGroup, hitboxIndex, hurtboxes[hurtboxIndex]) == false)
            {
                return false;
            }
            //AddCollidedHurtable(hitboxGroup.ID, hurtboxes[hurtboxIndex]);
            //HurtHurtbox(hitboxGroup, hitboxIndex, hurtboxes[hurtboxIndex], attacker, lch[hurtboxIndex].Point);
            return true;
        }*/
        #endregion
        }
}
