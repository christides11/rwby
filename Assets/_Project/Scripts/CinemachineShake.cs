using UnityEngine;
using Cinemachine;

namespace rwby
{
    public class CinemachineShake : MonoBehaviour
    {
        [SerializeField] protected CinemachineVirtualCamera virtualCamera;
        [System.NonSerialized] protected CinemachineBasicMultiChannelPerlin perlin;
        
        [SerializeField] protected AnimationCurve shakeCurve;

        private void Awake()
        {
            perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        public void ShakeCamera(float intensity, float time)
        {
            perlin.m_AmplitudeGain = intensity * shakeCurve.Evaluate(time);
        }

        public void Reset()
        {
            perlin.m_AmplitudeGain = 0;
        }
    }
}