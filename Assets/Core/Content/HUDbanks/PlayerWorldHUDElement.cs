using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class PlayerWorldHUDElement : HUDElement
    {
        public PlayerWorldHUD worldHUDPrefab;

        public LayerMask playerLayerMask;
        public float checkTime = 0.5f;
        public float checkRadius = 20;

        private Collider[] checkResults = new Collider[20];
        
        private Dictionary<FighterManager, PlayerWorldHUD> objectsTracked =
            new Dictionary<FighterManager, PlayerWorldHUD>();

        private float time = 0;
        public override void UpdateElement(BaseHUD parentHUD)
        {
            base.UpdateElement(parentHUD);
            if (parentHUD.playerFighter == null)
            {
                return;
            }

            UpdateLookDirection(parentHUD);
            
            time += Time.deltaTime;
            if (time < checkTime) return;
            time = 0;

            int hitCount = parentHUD.playerFighter.Runner.GetPhysicsScene().OverlapSphere(parentHUD.playerFighter.myTransform.position,
                checkRadius, checkResults, playerLayerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                FighterManager fm = checkResults[i].GetComponent<FighterManager>();
                if (fm == parentHUD.playerFighter || objectsTracked.ContainsKey(fm)) continue;

                AddTrackedObject(fm);
            }

            CleanupInvalidObjects();
            
            System.Array.Clear(checkResults, 0, checkResults.Length);
        }

        private void UpdateLookDirection(BaseHUD parentHUD)
        {
            foreach (PlayerWorldHUD pwhud in objectsTracked.Values)
            {
                pwhud.transform.LookAt(parentHUD.cameraSwitcher.cam.transform.position);
                pwhud.UpdateHUD();
            }
        }

        List<FighterManager> objectsToRemove = new List<FighterManager>(5);
        private void CleanupInvalidObjects()
        {
            foreach (var ob in objectsTracked)
            {
                bool objectisBeingHit = false;
                for (int i = 0; i < checkResults.Length; i++)
                {
                    if (checkResults[i] != null && checkResults[i].transform == ob.Key.transform)
                    {
                        objectisBeingHit = true;
                        break;
                    }
                }

                if (objectisBeingHit) continue;
                
                objectsToRemove.Add(ob.Key);
            }
            
            foreach (var fm in objectsToRemove)
            {
                RemoveTrackedObject(fm);
            }
            objectsToRemove.Clear();;
        }

        private void RemoveTrackedObject(FighterManager fm)
        {
            Destroy(objectsTracked[fm].gameObject);
            objectsTracked.Remove(fm);
        }

        private void AddTrackedObject(FighterManager fm)
        {
            PlayerWorldHUD pwhud = GameObject.Instantiate(worldHUDPrefab, fm.visualTransform, false);
            pwhud.transform.localPosition = new Vector3(0, 2, 0);
            pwhud.Setup(fm);
            objectsTracked.Add(fm, pwhud);
        }
    }
}