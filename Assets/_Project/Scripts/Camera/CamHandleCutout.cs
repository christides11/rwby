using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    [OrderAfter(typeof(PlayerCamera))]
    public class CamHandleCutout : SimulationBehaviour
    {
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private Camera camera;
        [SerializeField] private LayerMask wallMask;

        private RaycastHit[] hitObjects = new RaycastHit[5];
        private MaterialPropertyBlock propBlock;

        public float cutoutSize = 0.15f;
        public float falloffSize = 0.02f;
        public float cutoutCloseSpeed = 1.0f;
        public float cutoutOpenSpeed = 1.0f;

        [SerializeField] private Vector3 targetOffset = Vector3.up;
        
        private Dictionary<Renderer, Coroutine> RunningCoroutines = new Dictionary<Renderer, Coroutine>();
        private List<Renderer> ObjectsBlockingView = new List<Renderer>();

        //[SerializeField] private float cutoutCooldown = 0.0f;
        //private float cooldown;
        
        private void Awake()
        {
            propBlock = new MaterialPropertyBlock();
        }
        
        public override void Render()
        {
            base.Render();
            if (!playerCamera.FollowTarget) return;

            Vector2 cutoutPos = camera.WorldToViewportPoint(playerCamera.FollowTarget.position + targetOffset);
            cutoutPos.y /= (Screen.width / Screen.height);

            Vector3 offset = (playerCamera.FollowTarget.position + targetOffset) - transform.position;
            int hitCount = Runner.GetPhysicsScene().Raycast(transform.position, offset, hitObjects, offset.magnitude, wallMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Renderer r = hitObjects[i].transform.GetComponent<Renderer>();
                if (ObjectsBlockingView.Contains(r)) continue;

                if (RunningCoroutines.ContainsKey(r))
                {
                    if(RunningCoroutines[r] != null) StopCoroutine(RunningCoroutines[r]);
                    RunningCoroutines.Remove(r);
                }
                
                RunningCoroutines.Add(r, StartCoroutine(OpenCutout(r)));
                ObjectsBlockingView.Add(r);
            }

            CloseCutoutObjectsNoLongerHit();

            ClearHits();
            
            for (int i = 0; i < ObjectsBlockingView.Count; i++)
            {
                var renderer = ObjectsBlockingView[i];
                renderer.GetPropertyBlock(propBlock);
                propBlock.SetVector("_CutoutPos", cutoutPos);
                renderer.SetPropertyBlock(propBlock);
            }
        }

        List<Renderer> objectsToRemove = new List<Renderer>(5);
        private void CloseCutoutObjectsNoLongerHit()
        {
            foreach (var renderer in ObjectsBlockingView)
            {
                bool objectisBeingHit = false;
                for (int i = 0; i < hitObjects.Length; i++)
                {
                    if (hitObjects[i].transform != null && hitObjects[i].transform == renderer.transform)
                    {
                        objectisBeingHit = true;
                        break;
                    }
                }

                if (objectisBeingHit) continue;

                if (RunningCoroutines.ContainsKey(renderer))
                {
                    if(RunningCoroutines[renderer] != null) StopCoroutine(RunningCoroutines[renderer]);
                    RunningCoroutines.Remove(renderer);
                }
                RunningCoroutines.Add(renderer, StartCoroutine(CloseCutout(renderer)));
                objectsToRemove.Add(renderer);
            }

            foreach (var renderer in objectsToRemove)
            {
                ObjectsBlockingView.Remove(renderer);
            }
            objectsToRemove.Clear();;
        }

        private IEnumerator CloseCutout(Renderer renderer)
        {
            renderer.GetPropertyBlock(propBlock);
            float time = 0;
            var s = propBlock.GetFloat("_CutoutSize");

            while (s > 0)
            {
                renderer.GetPropertyBlock(propBlock);
                s = Mathf.Lerp(cutoutSize, 0.0f, time * cutoutCloseSpeed);
                propBlock.SetFloat("_CutoutSize", s);
                propBlock.SetFloat("_FalloffSize", falloffSize);
                renderer.SetPropertyBlock(propBlock);

                time += Time.deltaTime;
                yield return null;
            }

            if (!RunningCoroutines.ContainsKey(renderer)) yield break;
            StopCoroutine(RunningCoroutines[renderer]);
            RunningCoroutines.Remove(renderer);
        }

        private IEnumerator OpenCutout(Renderer renderer)
        {
            renderer.GetPropertyBlock(propBlock);
            float time = 0;
            var s = propBlock.GetFloat("_CutoutSize");

            while (s < cutoutSize)
            {
                renderer.GetPropertyBlock(propBlock);
                s = Mathf.Lerp( 0.0f, cutoutSize, time * cutoutOpenSpeed);
                propBlock.SetFloat("_CutoutSize", s);
                propBlock.SetFloat("_FalloffSize", falloffSize);
                renderer.SetPropertyBlock(propBlock);

                time += Time.deltaTime;
                yield return null;
            }

            if (!RunningCoroutines.ContainsKey(renderer)) yield break;
            StopCoroutine(RunningCoroutines[renderer]);
            RunningCoroutines.Remove(renderer);
        }

        private void ClearHits()
        {
            System.Array.Clear(hitObjects, 0, hitObjects.Length);
        }
    }
}