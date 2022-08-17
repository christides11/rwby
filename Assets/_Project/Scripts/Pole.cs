using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class Pole : MonoBehaviour
    {
        public Transform startTransform;
        public Transform endTransform;

        public Vector3 GetNearestPoint(Vector3 point)
        {
            var position = startTransform.position;
            var line = (endTransform.position - position);
            var len = line.magnitude;
            line.Normalize();
   
            var v = point - position;
            var d = Vector3.Dot(v, line);
            d = Mathf.Clamp(d, 0f, len);
            return position + line * d;
        }

        public Vector3 GetNearestFaceDirection(Vector3 dir)
        {
            float angle = Vector3.Angle(startTransform.forward, dir);
            return angle < 90 ? startTransform.forward : -startTransform.forward;
        }
    }
}