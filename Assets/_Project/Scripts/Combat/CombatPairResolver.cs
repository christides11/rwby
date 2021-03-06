using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using HnSF.Combat;
using System.Linq;

namespace rwby
{
    [OrderAfter(typeof(CombatPairFinder), typeof(GameModeBase))]
    public class CombatPairResolver : SimulationBehaviour
    {
        public CombatPairFinder pairFinder;

        private HashSet<(NetworkObject, NetworkObject)> resolvedPairs = new HashSet<(NetworkObject, NetworkObject)>();

        public override void FixedUpdateNetwork()
        {
            HitboxPairResolver();
            resolvedPairs.Clear();
        }

        private void HitboxPairResolver()
        {
            foreach (var pair in pairFinder.hitboxCombatPairs)
            {
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
                }
                else if (pair.Value.result == CombatPairFinder.HitboxCombatResult.HitHurtbox)
                {
                    Debug.Log("P1 Win.");
                    var objHitInfo = pair.Key.Item1.GetComponent<IAttacker>().BuildHurtInfo(pair.Value.attackerHitbox, pair.Value.attackeeHurtbox);
                    pair.Key.Item1.GetComponent<IAttacker>().DoHit(pair.Value.attackerHitbox, pair.Value.attackeeHurtbox, objHitInfo);
                }
                else if (oppositePair.result == CombatPairFinder.HitboxCombatResult.HitHurtbox)
                {
                    Debug.Log("P2 Win.");
                    var objHitInfo = pair.Key.Item2.GetComponent<IAttacker>().BuildHurtInfo(oppositePair.attackerHitbox, oppositePair.attackeeHurtbox);
                    pair.Key.Item2.GetComponent<IAttacker>().DoHit(oppositePair.attackerHitbox, oppositePair.attackeeHurtbox, objHitInfo);
                }
                else
                {
                    pair.Key.Item1.GetComponent<IAttacker>().DoClash(pair.Value.attackerHitbox, pair.Value.attackeeHitbox);
                    pair.Key.Item2.GetComponent<IAttacker>().DoClash(oppositePair.attackerHitbox, oppositePair.attackeeHitbox);
                }
                
                resolvedPairs.Add(pair.Key);
                resolvedPairs.Add(oppositePairKey);
            }

            pairFinder.hitboxCombatPairs.Clear();
            pairFinder.throwboxCombatPairs.Clear();
        }
    }
}