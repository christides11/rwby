using Fusion;
using HnSF.Combat;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [OrderAfter(typeof(Fusion.HitboxManager))]
    [OrderBefore(typeof(CombatPairFinder))]
    public class EntityBoxManager : NetworkBehaviour, IBoxCollection
    {
        public CustomHitbox[] Hitboxes { get { return hitboxes; } }
        public Hurtbox[] Hurtboxes { get { return hurtboxes; } }
        public Collbox[] Collboxes { get { return collisionboxes; } }
        public CustomHitbox[] Throwboxes { get { return throwboxes; } }

        public FighterManager manager;
        public NetworkObject networkObject;
        public IHurtable hurtable;
        public HitboxRoot hRoot;
        [HideInInspector] public Settings settings;

        public CustomHitbox[] hitboxes;
        public Hurtbox[] hurtboxes;
        [FormerlySerializedAs("collboxes")] public Collbox[] collisionboxes;
        public Throwablebox[] throwableboxes;
        public CustomHitbox[] throwboxes;

        [Networked] public EntityBoxesDefinition currentBoxesDefinition { get; set; }

        public bool dirty = true;
        
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
            foreach (var thb in throwboxes)
            {
                thb.ownerNetworkObject = networkObject;
            }

            dirty = true;
        }
        
        public void ResetAllBoxes()
        {
            var tempBoxesDefinition = currentBoxesDefinition;
            ResetBoxesArray(tempBoxesDefinition.hurtboxes);
            ResetBoxesArray(tempBoxesDefinition.hitboxes);
            ResetBoxesArray(tempBoxesDefinition.colboxes);
            ResetBoxesArray(tempBoxesDefinition.throwboxes);
            ResetBoxesArray(tempBoxesDefinition.throwableboxes);
            currentBoxesDefinition = tempBoxesDefinition;
            dirty = true;
        }

        private void ResetBoxesArray(NetworkArray<EntityBoxDefinition> boxDefinitions)
        {
            for (int i = 0; i < boxDefinitions.Length; i++)
            {
                boxDefinitions.Set(i, new EntityBoxDefinition());
            }
        }

        private int currHurtboxCnt = 0;
        private int currHitboxCnt = 0;
        private int currCollboxCnt = 0;
        private int currThrowableboxCnt = 0;
        private int currThrowboxCnt = 0;
        public override void FixedUpdateNetwork()
        {
            if (dirty)
            {
                SyncBoxes();
                dirty = false;
            }
            CombatPairFinder.singleton.RegisterObject(Object);
            currHurtboxCnt = 0;
            currHitboxCnt = 0;
            currCollboxCnt = 0;
            currThrowableboxCnt = 0;
            currThrowboxCnt = 0;
        }

        private void SyncBoxes()
        {
            SyncBoxList(hurtboxes, currentBoxesDefinition.hurtboxes);
            SyncBoxList(hitboxes, currentBoxesDefinition.hitboxes);
            SyncBoxList(collisionboxes, currentBoxesDefinition.colboxes);
            SyncBoxList(throwboxes, currentBoxesDefinition.throwboxes);
            SyncBoxList(throwableboxes, currentBoxesDefinition.throwableboxes);
        }

        private void SyncBoxList(CustomHitbox[] boxList, NetworkArray<EntityBoxDefinition> entityBoxDefinitions)
        {
            for (int i = 0; i < boxList.Length; i++)
            {
                if (entityBoxDefinitions[i].extents == Vector3.zero)
                {
                    hRoot.SetHitboxActive(boxList[i], false);
                    boxList[i].SetBoxActiveState(false);
                    continue;
                }
                hRoot.SetHitboxActive(boxList[i], true);
                boxList[i].SetBoxActiveState(true);
                switch (entityBoxDefinitions[i].shape)
                {
                    case BoxShape.Rectangle:
                        boxList[i].SetBoxSize(entityBoxDefinitions[i].offset, entityBoxDefinitions[i].extents);
                        break;
                    case BoxShape.Circle:
                        boxList[i].SetSphereSize(entityBoxDefinitions[i].offset, entityBoxDefinitions[i].extents.x);
                        break;
                }
            }
        }

        public void AddBox(FighterBoxType boxType, int attachedTo, BoxShape shape, Vector3 offset, Vector3 boxExtents, float sphereRadius, 
            int definitionIndex, IBoxDefinitionCollection definition)
        {
            void NewFunction(NetworkArray<EntityBoxDefinition> boxList, CustomHitbox[] customBoxList, int index)
            {
                boxList.Set(index, new EntityBoxDefinition()
                {
                    extents = shape == BoxShape.Circle ? new Vector3(sphereRadius, 0, 0) : boxExtents,
                    offset = offset,
                    shape = shape
                });
                customBoxList[index].definition = definition;
                customBoxList[index].definitionIndex = definitionIndex;
            }

            var tempBoxesDefinition = currentBoxesDefinition;
            switch (boxType)
            {
                case FighterBoxType.Hurtbox:
                    NewFunction(tempBoxesDefinition.hurtboxes, hurtboxes, currHurtboxCnt);
                    currHurtboxCnt++;
                    break;
                case FighterBoxType.Hitbox:
                    NewFunction(tempBoxesDefinition.hitboxes, hitboxes, currHitboxCnt);
                    currHitboxCnt++;
                    break;
                case FighterBoxType.Throwablebox:
                    NewFunction(tempBoxesDefinition.throwableboxes, throwableboxes, currThrowableboxCnt);
                    currThrowableboxCnt++;
                    break;
                case FighterBoxType.Throwbox:
                    NewFunction(tempBoxesDefinition.throwboxes, throwboxes, currThrowboxCnt);
                    currThrowboxCnt++;
                    break;
                case FighterBoxType.Collisionbox:
                    NewFunction(tempBoxesDefinition.colboxes, collisionboxes, currCollboxCnt);
                    currCollboxCnt++;
                    break;
            }
            currentBoxesDefinition = tempBoxesDefinition;
            dirty = true;
        }
    }
}