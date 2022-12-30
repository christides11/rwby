using UnityEngine;

namespace rwby
{
    public class FighterHitManager : EntityHitManager
    {
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCombatManager combatManager;
        
        public virtual void Reset()
        {
            hitObjects.Clear();
            for (int i = 0; i < hitboxGroupHitCounts.Length; i++)
            {
                hitboxGroupHitCounts.Set(i, 0);
                hitboxGroupBlockedCounts.Set(i, 0);
            }
        }

        public override bool IsHitHurtboxValid(CustomHitbox atackerHitbox, Hurtbox h)
        {
            if (h.ownerNetworkObject == Object) return false;
            for(int i = 0; i < hitObjects.Count; i++)
            {
                if(hitObjects[i].collisionType == IDGroupCollisionType.Hurtbox
                    && hitObjects[i].hitIHurtableNetID == h.ownerNetworkObject.Id
                    && hitObjects[i].hitByIDGroup == atackerHitbox.definition.HitboxInfo[atackerHitbox.definitionIndex].ID)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool IsHitHitboxValid(CustomHitbox attackerHitbox, CustomHitbox h)
        {
            if (h.ownerNetworkObject == Object) return false;
            if (attackerHitbox.definition.HitboxInfo[attackerHitbox.definitionIndex].clashLevel
                != h.definition.HitboxInfo[h.definitionIndex].clashLevel) return false;
            for (int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i].collisionType == IDGroupCollisionType.Hitbox
                    && hitObjects[i].hitIHurtableNetID == h.ownerNetworkObject.Id
                    && hitObjects[i].hitByIDGroup == attackerHitbox.definition.HitboxInfo[attackerHitbox.definitionIndex].ID)
                {
                    return false;
                }
            }
            return true;
        }
        
        public override void HandleHitReaction(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo, HitInfo hi,
            HitReaction hitReaction)
        {
            base.HandleHitReaction(hitbox, enemyHurtbox, hurtInfo, hi, hitReaction);
            combatManager.SetHitStop(hitReaction.hitInfoGroup.attackerHitstop);
            if (!string.IsNullOrEmpty(hitReaction.hitInfoGroup.hitEffect))
            {
                manager.fighterEffector.AddEffects(new []{ new EffectReference()
                {
                    effect = hitReaction.hitInfoGroup.hitEffect,
                    effectbank = hitReaction.hitInfoGroup.hitEffectbank,
                    offset = enemyHurtbox.Position,
                    parent = new FighterBoneReferenceRaw(){ value = 0 },
                    rotation = manager.myTransform.eulerAngles,
                    scale = new Vector3(1, 1, 1),
                    autoIncrement = true
                } }, addToEffectSet: false);
            }

            if (!string.IsNullOrEmpty(hitReaction.hitInfoGroup.hitSound))
            {
                manager.fighterSounder.AddSFXs(new []{ new SoundReference()
                {
                    soundbank = hitReaction.hitInfoGroup.hitSoundbank,
                    sound = hitReaction.hitInfoGroup.hitSound,
                    maxDist = hitReaction.hitInfoGroup.hitSoundMaxDist,
                    minDist = hitReaction.hitInfoGroup.hitSoundMinDist,
                    offset = enemyHurtbox.Position,
                    parented = false,
                    volume = hitReaction.hitInfoGroup.hitSoundVolume
                }});
            }
            
            manager.shakeDefinition = new CmaeraShakeDefinition()
            {
                shakeStrength = hitReaction.hitInfoGroup.hitCameraShakeStrength,
                startFrame = Runner.Tick,
                endFrame = Runner.Tick + hitReaction.hitInfoGroup.cameraShakeLength
            };
        }

        public virtual void HandleDirectHitReaction(HitInfo hi, HitReaction hitReaction)
        {
            combatManager.SetHitStop(hitReaction.hitInfoGroup.attackerHitstop);
            if (!string.IsNullOrEmpty(hitReaction.hitInfoGroup.hitEffect))
            {
                manager.fighterEffector.AddEffects(new []{ new EffectReference()
                {
                    effect = hitReaction.hitInfoGroup.hitEffect,
                    effectbank = hitReaction.hitInfoGroup.hitEffectbank,
                    offset = manager.myTransform.position,
                    parent = new FighterBoneReferenceRaw(){ value = 0 },
                    rotation = manager.myTransform.eulerAngles,
                    scale = new Vector3(1, 1, 1),
                    autoIncrement = true
                } }, addToEffectSet: false);
            }

            if (!string.IsNullOrEmpty(hitReaction.hitInfoGroup.hitSound))
            {
                manager.fighterSounder.AddSFXs(new []{ new SoundReference()
                {
                    soundbank = hitReaction.hitInfoGroup.hitSoundbank,
                    sound = hitReaction.hitInfoGroup.hitSound,
                    maxDist = hitReaction.hitInfoGroup.hitSoundMaxDist,
                    minDist = hitReaction.hitInfoGroup.hitSoundMinDist,
                    offset = manager.myTransform.position,
                    parented = false,
                    volume = hitReaction.hitInfoGroup.hitSoundVolume
                }});
            }
            
            manager.shakeDefinition = new CmaeraShakeDefinition()
            {
                shakeStrength = hitReaction.hitInfoGroup.hitCameraShakeStrength,
                startFrame = Runner.Tick,
                endFrame = Runner.Tick + hitReaction.hitInfoGroup.cameraShakeLength
            };
        }

        public override void HandleBlockReaction(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo,
            HitInfo hi, HitReaction hitReaction)
        {
            base.HandleBlockReaction(hitbox, enemyHurtbox, hurtInfo, hi, hitReaction);
            combatManager.SetHitStop(hitReaction.hitInfoGroup.attackerHitstop);
            if (!string.IsNullOrEmpty(hitReaction.hitInfoGroup.blockEffect))
            {
                manager.fighterEffector.AddEffects(new []{ new EffectReference()
                {
                    effect = hitReaction.hitInfoGroup.blockEffect,
                    effectbank = hitReaction.hitInfoGroup.blockEffectbank,
                    offset = enemyHurtbox.Position,
                    parent = new FighterBoneReferenceRaw(){ value = 0 },
                    rotation = enemyHurtbox.ownerNetworkObject.gameObject.transform.eulerAngles,
                    scale = new Vector3(1, 1, 1),
                    autoIncrement = true
                } }, addToEffectSet: false);
            }
            
            if (!string.IsNullOrEmpty(hitReaction.hitInfoGroup.hitBlockSound))
            {
                manager.fighterSounder.AddSFXs(new []{ new SoundReference()
                {
                    soundbank = hitReaction.hitInfoGroup.hitBlockSoundbank,
                    sound = hitReaction.hitInfoGroup.hitBlockSound,
                    maxDist = hitReaction.hitInfoGroup.hitSoundMaxDist,
                    minDist = hitReaction.hitInfoGroup.hitSoundMinDist,
                    offset = enemyHurtbox.Position,
                    parented = false,
                    volume = hitReaction.hitInfoGroup.hitSoundVolume
                }});
            }
            
            manager.shakeDefinition = new CmaeraShakeDefinition()
            {
                shakeStrength = hitReaction.hitInfoGroup.hitCameraShakeStrength,
                startFrame = Runner.Tick,
                endFrame = Runner.Tick + hitReaction.hitInfoGroup.cameraShakeLength
            };

            manager.FPhysicsManager.forceMovement += hitReaction.pushback;
        }

        // TODO: Visual effects, better handling.
        public override void DoClash(CustomHitbox hitbox, CustomHitbox enemyHitbox)
        {
            base.DoClash(hitbox, enemyHitbox);
            combatManager.ResetString();
            combatManager.ClashState = true;
            combatManager.SetHitStop(17);
            
            manager.shakeDefinition = new CmaeraShakeDefinition()
            {
                shakeStrength = CameraShakeStrength.Medium,
                startFrame = Runner.Tick,
                endFrame = Runner.Tick + 17
            };
        }

        public override HurtInfo BuildHurtInfo(CustomHitbox hitbox, Hurtbox hurtbox)
        {
            HurtInfo hi = base.BuildHurtInfo(hitbox, hurtbox);
            hi.team = combatManager.GetTeam();
            hi.attackerVelocity = manager.FPhysicsManager.GetOverallForce();
            return hi;
        }
    }
}
