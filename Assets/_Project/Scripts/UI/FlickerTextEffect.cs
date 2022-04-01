using TMPro;
using UnityEngine;

namespace rwby
{
    public class FlickerTextEffect : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;
        public Color defaultColor;
        public Color endColor;
        public float timeMultiplier = 1.0f;
        public float pingPongLength = 1.0f;
        
        private void Update()
        {
            textMesh.color = Color.Lerp(defaultColor, endColor, Mathf.PingPong(Time.time * timeMultiplier, pingPongLength));
        }
    }
}