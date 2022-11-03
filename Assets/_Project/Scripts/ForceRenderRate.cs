using System.Collections;
using System.Threading;
using UnityEngine;

namespace rwby
{
    public class ForceRenderRate : MonoBehaviour
    {
        public float Rate = 60.0f;
        
        private float currentFrameTime;
        private IEnumerator enumerator;
        
        public void Activate(int cap)
        {
            Rate = cap;
            if (enumerator != null) return;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 9999;
            currentFrameTime = Time.realtimeSinceStartup;
            enumerator = WaitForNextFrame();
            StartCoroutine(enumerator);
        }

        public void Deactivate()
        {
            if (enumerator == null) return;
            StopCoroutine(enumerator);
            enumerator = null;
        }

        IEnumerator WaitForNextFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                currentFrameTime += 1.0f / Rate;
                var t = Time.realtimeSinceStartup;
                var sleepTime = currentFrameTime - t - 0.01f;
                if (sleepTime > 0)
                    Thread.Sleep((int)(sleepTime * 1000));
                while (t < currentFrameTime)
                    t = Time.realtimeSinceStartup;
            }
        }
    }
}