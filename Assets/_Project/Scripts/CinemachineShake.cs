using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace rwby
{
    public class CinemachineShake : MonoBehaviour
    {
        [SerializeField] protected CinemachineVirtualCamera virtualCamera;
        [SerializeField] protected CinemachineBasicMultiChannelPerlin perlin;

        protected float shakeTime;
        protected float timer;

        [SerializeField] protected float shakeIntensity;
        [SerializeField] protected AnimationCurve shakeCurve;

        private void Awake()
        {
            perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        private void Update()
        {
            if (shakeTime == 0) return;

            perlin.m_AmplitudeGain = shakeIntensity * shakeCurve.Evaluate(timer/shakeTime);
            timer += Time.deltaTime;
            if(timer > shakeTime)
            {
                timer = 0;
                shakeTime = 0;
                perlin.m_AmplitudeGain = 0;
            }
        }

        public void ShakeCamera(float intensity, float time)
        {
            shakeIntensity = intensity;
            perlin.m_AmplitudeGain = intensity * shakeCurve.Evaluate(0);
            shakeTime = time;
            timer = 0;
        }
    }
}