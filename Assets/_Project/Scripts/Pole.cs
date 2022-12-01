using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

namespace rwby
{
    public class Pole : MonoBehaviour
    {
        public Transform startTransform;

        [FormerlySerializedAs("spine")] public SplineContainer spline;
        
        public Vector3 GetNearestPoint(Vector3 point)
        {
            SplineUtility.GetNearestPoint(spline.Spline, spline.transform.InverseTransformPoint(point), out var nearest, out float t);
            return spline.transform.TransformPoint(nearest);
        }

        public Vector3 GetNearestFaceDirection(Vector3 dir)
        {
            float angle = Vector3.Angle(startTransform.forward, dir);
            return angle < 90 ? startTransform.forward : -startTransform.forward;
        }
    }
}