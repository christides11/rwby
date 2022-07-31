using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace rwby
{
    [OrderAfter(typeof(FighterCombatManager))]
    [OrderBefore(typeof(GameModeBase))]
    public class CombatPairFinder : SimulationBehaviour
    {
        public enum HitboxCombatResult
        {
            None,
            HitHitbox,
            HitHurtbox
        }
        
        public struct HitboxCombatPair
        {
            public HitboxCombatResult result;
            public CustomHitbox attackerHitbox;
            public Hurtbox attackeeHurtbox;
            public CustomHitbox attackeeHitbox;
        }

        public struct CollisionPair
        {
            public Collbox boxa;
            public Collbox boxb;
        }

        public struct ThrowboxCombatPair
        {

        }

        public static CombatPairFinder singleton;

        public List<NetworkObject> broadphaseObjects = new List<NetworkObject>();

        // (Atacker, Atackee)
        public Dictionary<(NetworkObject, NetworkObject), HitboxCombatPair> hitboxCombatPairs = new Dictionary<(NetworkObject, NetworkObject), HitboxCombatPair>();
        public Dictionary<(NetworkObject, NetworkObject), ThrowboxCombatPair> throwboxCombatPairs = new Dictionary<(NetworkObject, NetworkObject), ThrowboxCombatPair>();
        public Dictionary<(NetworkObject, NetworkObject), CollisionPair> collisionPairs = new Dictionary<(NetworkObject, NetworkObject), CollisionPair>();

        public LayerMask hitboxLayermask;
        public LayerMask hurtboxLayermask;
        public LayerMask collboxLayermask;
        public LayerMask throwboxLayermask;

        private void Awake()
        {
            singleton = this;
        }

        public void RegisterObject(NetworkObject obj)
        {
            broadphaseObjects.Add(obj);
        }

        public override void FixedUpdateNetwork()
        {
            ResolveCollisionInteractions();
            ResolveHitInteractions();
            ResolveGrabInteractions();

            broadphaseObjects.Clear();
        }
        
        private void ResolveCollisionInteractions()
        {
            for (int i = broadphaseObjects.Count - 1; i >= 0; i--)
            {
                EntityBoxManager boxCollection = (EntityBoxManager)broadphaseObjects[i].GetComponent<IBoxCollection>();
                
                for (int b = 0; b < boxCollection.Collboxes.Length; b++)
                {
                    if (boxCollection.Collboxes[b].enabled == false) break;

                    int numHit = 0;
                    
                    switch (boxCollection.Collboxes[b].Type)
                    {
                        case HitboxTypes.Box:
                            numHit = Runner.LagCompensation.OverlapBox(boxCollection.Collboxes[b].transform.position,
                                boxCollection.Collboxes[b].BoxExtents, boxCollection.Collboxes[b].transform.rotation,
                                new PlayerRef(), hitsList, collboxLayermask);
                            break;
                        case HitboxTypes.Sphere:
                            numHit = Runner.LagCompensation.OverlapSphere(boxCollection.Collboxes[b].transform.position,
                                boxCollection.Collboxes[b].SphereRadius,
                                new PlayerRef(), hitsList, collboxLayermask);
                            break;
                    }

                    for (int f = 0; f < numHit; f++)
                    {
                        Collbox h = hitsList[f].GameObject.GetComponent<Collbox>();
                        if (!h.HitboxActive || h.ownerNetworkObject == broadphaseObjects[i]) continue;
                        if (collisionPairs.ContainsKey((h.ownerNetworkObject,
                                broadphaseObjects[i].GetBehaviour<NetworkObject>()))
                            || collisionPairs.ContainsKey((broadphaseObjects[i].GetBehaviour<NetworkObject>(),
                                h.ownerNetworkObject))) continue;
                        collisionPairs.Add((broadphaseObjects[i].GetBehaviour<NetworkObject>(), h.ownerNetworkObject),
                            new CollisionPair()
                            {
                                boxa = boxCollection.Collboxes[b],
                                boxb = h
                            });
                    }
                }
            }
        }

        private List<LagCompensatedHit> hitsList = new List<LagCompensatedHit>();
        private void ResolveHitInteractions()
        {
            for(int i = broadphaseObjects.Count-1; i >= 0; i--)
            {
                IBoxCollection boxCollection = broadphaseObjects[i].GetComponent<IBoxCollection>();

                bool objHasHitboxes = boxCollection.Hitboxes[0].HitboxActive == true;
                if (objHasHitboxes == false) continue;
                
                for (int a = 0; a < boxCollection.Hitboxes.Length; a++)
                {
                    if (boxCollection.Hitboxes[a].HitboxActive == false) break;

                    // Hitbox-Hurtbox Interaction Checking
                    int numHit = 0;
                    switch (boxCollection.Hitboxes[a].Type)
                    {
                        case HitboxTypes.Box:
                            numHit = Runner.LagCompensation.OverlapBox(boxCollection.Hitboxes[a].transform.position,
                                boxCollection.Hitboxes[a].BoxExtents, boxCollection.Hitboxes[a].transform.rotation,
                                new PlayerRef(), hitsList, hurtboxLayermask);
                            break;
                        case HitboxTypes.Sphere:
                            numHit = Runner.LagCompensation.OverlapSphere(boxCollection.Hitboxes[a].transform.position,
                                boxCollection.Hitboxes[a].SphereRadius,
                                new PlayerRef(), hitsList, hurtboxLayermask);
                            break;
                    }

                    for (int f = 0; f < numHit; f++)
                    {
                        Hurtbox h = hitsList[f].GameObject.GetComponent<Hurtbox>();
                        if (h.HitboxActive == true && broadphaseObjects[i].GetComponent<IAttacker>().IsHitHurtboxValid(boxCollection.Hitboxes[a], h))
                        {
                            var tuple = (broadphaseObjects[i].GetBehaviour<NetworkObject>(), h.ownerNetworkObject);
                            if (hitboxCombatPairs.ContainsKey(tuple))
                            {
                                if (hitboxCombatPairs[tuple].attackerHitbox.definition.HitboxInfo[hitboxCombatPairs[tuple].attackerHitbox.definitionIndex].ID 
                                    > boxCollection.Hitboxes[a].definition.HitboxInfo[boxCollection.Hitboxes[a].definitionIndex].ID)
                                {
                                    hitboxCombatPairs[tuple] = new HitboxCombatPair()
                                    {
                                        result = HitboxCombatResult.HitHurtbox,
                                        attackerHitbox = boxCollection.Hitboxes[a],
                                        attackeeHurtbox = h
                                    };
                                }
                            }
                            else
                            {
                                hitboxCombatPairs.Add(tuple,
                                    new HitboxCombatPair()
                                    {
                                        result = HitboxCombatResult.HitHurtbox,
                                        attackerHitbox = boxCollection.Hitboxes[a],
                                        attackeeHurtbox = h
                                    });
                            }
                        }
                    }

                    // Hitbox-Hitbox Interaction Checking
                    int hitboxNumHit = 0;
                    switch (boxCollection.Hitboxes[a].Type)
                    {
                        case HitboxTypes.Box:
                            hitboxNumHit = Runner.LagCompensation.OverlapBox(boxCollection.Hitboxes[a].transform.position,
                                boxCollection.Hitboxes[a].BoxExtents, boxCollection.Hitboxes[a].transform.rotation,
                                new PlayerRef(), hitsList, hitboxLayermask);
                            break;
                        case HitboxTypes.Sphere:
                            hitboxNumHit = Runner.LagCompensation.OverlapSphere(boxCollection.Hitboxes[a].transform.position,
                                boxCollection.Hitboxes[a].SphereRadius,
                                new PlayerRef(), hitsList, hitboxLayermask);
                            break;
                    }

                    for(int g = 0; g < hitboxNumHit; g++)
                    {
                        CustomHitbox h = hitsList[g].GameObject.GetComponent<CustomHitbox>();
                        if (h.HitboxActive == true && broadphaseObjects[i].GetComponent<IAttacker>().IsHitHitboxValid(boxCollection.Hitboxes[a], h))
                        {
                            var tuple = (broadphaseObjects[i].GetBehaviour<NetworkObject>(), h.ownerNetworkObject);
                            if (hitboxCombatPairs.ContainsKey(tuple))
                            {
                                if(hitboxCombatPairs[tuple].result == HitboxCombatResult.HitHitbox
                                    && hitboxCombatPairs[tuple].attackeeHitbox.definition.HitboxInfo[hitboxCombatPairs[tuple].attackeeHitbox.definitionIndex].ID 
                                    > h.definition.HitboxInfo[h.definitionIndex].ID)
                                {
                                    HitboxCombatPair temp = hitboxCombatPairs[tuple];
                                    temp.attackeeHitbox = h;
                                    hitboxCombatPairs[tuple] = temp;
                                }
                            }
                            else
                            {
                                hitboxCombatPairs.Add(tuple,
                                    new HitboxCombatPair()
                                    {
                                        result = HitboxCombatResult.HitHitbox,
                                        attackerHitbox = boxCollection.Hitboxes[a],
                                        attackeeHitbox = h
                                    });
                            }
                        }
                    }
                }
            }
        }

        private void ResolveGrabInteractions()
        {

        }
    }
}