using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    [OrderAfter(typeof(CombatPairFinder), typeof(GameModeBase))]
    public class CombatPairResolver : SimulationBehaviour
    {
        public CombatPairFinder pairFinder;

        private HashSet<(NetworkObject, NetworkObject)> resolvedPairs = new HashSet<(NetworkObject, NetworkObject)>();
        private HashSet<NetworkObject> receiversResolved = new HashSet<NetworkObject>();

        public float collisionSolvePercentage = 1.0f;

        public override void FixedUpdateNetwork()
        {
            CollisionPairResolver();
            HitboxPairResolver();
            ThrowPairResolver();
            resolvedPairs.Clear();
            receiversResolved.Clear();
        }

        private void ThrowPairResolver()
        {
            foreach (var pair in pairFinder.throwboxCombatPairs)
            {
                if(receiversResolved.Contains(pair.Key.Item2)) continue;
                
                pair.Key.Item2.GetComponent<IThrowee>().ThroweeInitilization(pair.Key.Item1);
                pair.Key.Item1.GetComponent<IThrower>().ThrowerInitilization(pair.Key.Item2);
            }
            
            pairFinder.throwboxCombatPairs.Clear();
        }

        private void CollisionPairResolver()
        {
            foreach (var pair in pairFinder.collisionPairs)
            {
                Vector3 direction;
                float distance;
                bool overlapped = Physics.ComputePenetration(
                    pair.Value.boxa.boxCollider, pair.Value.boxa.transform.position, pair.Value.boxa.transform.rotation,
                    pair.Value.boxb.boxCollider, pair.Value.boxb.transform.position, pair.Value.boxb.transform.rotation,
                    out direction, out distance
                );
                
                if (overlapped)
                {
                    direction.y = 0;
                    direction.Normalize();
                    pair.Key.Item1.GetComponent<FighterPhysicsManager>().kCC.Motor.SetPosition(pair.Key.Item1.transform.position + (direction * (distance/2.0f) * collisionSolvePercentage), false);
                    pair.Key.Item2.GetComponent<FighterPhysicsManager>().kCC.Motor.SetPosition(pair.Key.Item2.transform.position - (direction * (distance/2.0f) * collisionSolvePercentage), false);
                }
            }
            pairFinder.collisionPairs.Clear();
        }

        private void HitboxPairResolver()
        {
            foreach (var pair in pairFinder.hitboxCombatPairs)
            {
                if(receiversResolved.Contains(pair.Key.Item2)) continue; // NetworkObject was already hit this frame by something.
                if (resolvedPairs.Contains((pair.Key.Item1, pair.Key.Item2))) continue;
                var oppositePairKey = (pair.Key.Item2, pair.Key.Item1);
                CombatPairFinder.HitboxCombatPair oppositePair = new CombatPairFinder.HitboxCombatPair() { result = CombatPairFinder.HitboxCombatResult.None };
                if (pairFinder.hitboxCombatPairs.ContainsKey((pair.Key.Item2, pair.Key.Item1)))
                {
                    oppositePair = pairFinder.hitboxCombatPairs[(pair.Key.Item2, pair.Key.Item1)];
                }

                // No opposing result.
                if (oppositePair.result == CombatPairFinder.HitboxCombatResult.None)
                {
                    switch (pair.Value.result)
                    {
                        case CombatPairFinder.HitboxCombatResult.HitHurtbox:
                            pair.Key.Item1.GetComponent<IAttacker>().DoHit(pair.Value.attackerHitbox, pair.Value.attackeeHurtbox, 
                                pair.Key.Item1.GetComponent<IAttacker>().BuildHurtInfo(pair.Value.attackerHitbox, pair.Value.attackeeHurtbox));
                            break;
                        case CombatPairFinder.HitboxCombatResult.HitHitbox:
                            pair.Key.Item1.GetComponent<IAttacker>().DoClash(pair.Value.attackerHitbox, pair.Value.attackeeHitbox);
                            break;
                    }

                    resolvedPairs.Add( (pair.Key.Item1, pair.Key.Item2) );
                    receiversResolved.Add(pair.Key.Item2);
                    break;
                }

                if (pair.Value.result == CombatPairFinder.HitboxCombatResult.HitHurtbox && oppositePair.result == CombatPairFinder.HitboxCombatResult.HitHurtbox)
                {
                    // Trade.
                    Debug.Log("Trade.");
                    var objOneHitInfo = pair.Key.Item1.GetComponent<IAttacker>().BuildHurtInfo(pair.Value.attackerHitbox, pair.Value.attackeeHurtbox);
                    var objTwoHitInfo = pair.Key.Item2.GetComponent<IAttacker>().BuildHurtInfo(oppositePair.attackerHitbox, oppositePair.attackeeHurtbox);
                    pair.Key.Item1.GetComponent<IAttacker>().DoHit(pair.Value.attackerHitbox, pair.Value.attackeeHurtbox, objOneHitInfo);
                    pair.Key.Item2.GetComponent<IAttacker>().DoHit(oppositePair.attackerHitbox, oppositePair.attackeeHurtbox, objTwoHitInfo);
                    receiversResolved.Add(pair.Key.Item1);
                    receiversResolved.Add(pair.Key.Item2);
                }
                else if (pair.Value.result == CombatPairFinder.HitboxCombatResult.HitHurtbox)
                {
                    Debug.Log("P1 Win.");
                    var objHitInfo = pair.Key.Item1.GetComponent<IAttacker>().BuildHurtInfo(pair.Value.attackerHitbox, pair.Value.attackeeHurtbox);
                    pair.Key.Item1.GetComponent<IAttacker>().DoHit(pair.Value.attackerHitbox, pair.Value.attackeeHurtbox, objHitInfo);
                    receiversResolved.Add(pair.Key.Item2);
                }
                else if (oppositePair.result == CombatPairFinder.HitboxCombatResult.HitHurtbox)
                {
                    Debug.Log("P2 Win.");
                    var objHitInfo = pair.Key.Item2.GetComponent<IAttacker>().BuildHurtInfo(oppositePair.attackerHitbox, oppositePair.attackeeHurtbox);
                    pair.Key.Item2.GetComponent<IAttacker>().DoHit(oppositePair.attackerHitbox, oppositePair.attackeeHurtbox, objHitInfo);
                    receiversResolved.Add(pair.Key.Item1);
                }
                else
                {
                    pair.Key.Item1.GetComponent<IAttacker>().DoClash(pair.Value.attackerHitbox, pair.Value.attackeeHitbox);
                    pair.Key.Item2.GetComponent<IAttacker>().DoClash(oppositePair.attackerHitbox, oppositePair.attackeeHitbox);
                    receiversResolved.Add(pair.Key.Item1);
                    receiversResolved.Add(pair.Key.Item2);
                }
                
                resolvedPairs.Add(pair.Key);
                resolvedPairs.Add(oppositePairKey);
            }

            pairFinder.hitboxCombatPairs.Clear();
        }
    }
}