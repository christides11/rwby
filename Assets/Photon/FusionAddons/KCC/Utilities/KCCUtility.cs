namespace Fusion.KCC
{
	using UnityEngine;

	public static partial class KCCUtility
	{
		// PUBLIC METHODS

		public static void ClampLookRotationAngles(ref float pitch, ref float yaw)
		{
			pitch = Mathf.Clamp(pitch, -90.0f, 90.0f);

			while (yaw > 180.0f)
			{
				yaw -= 360.0f;
			}
			while (yaw < -180.0f)
			{
				yaw += 360.0f;
			}
		}

		public static void GetLookRotationAngles(Quaternion lookRotation, out float pitch, out float yaw)
		{
			Vector3 eulerAngles = lookRotation.eulerAngles;

			if (eulerAngles.x > 180.0f) { eulerAngles.x -= 360.0f; }
			if (eulerAngles.y > 180.0f) { eulerAngles.y -= 360.0f; }

			pitch = Mathf.Clamp(eulerAngles.x,  -90.0f,  90.0f);
			yaw   = Mathf.Clamp(eulerAngles.y, -180.0f, 180.0f);
		}

		public static Vector3 GetEulerLookRotation(Quaternion lookRotation)
		{
			Vector3 eulerAngles = lookRotation.eulerAngles;

			if (eulerAngles.x > 180.0f) { eulerAngles.x -= 360.0f; }
			if (eulerAngles.y > 180.0f) { eulerAngles.y -= 360.0f; }

			eulerAngles.x = Mathf.Clamp(eulerAngles.x,  -90.0f,  90.0f);
			eulerAngles.y = Mathf.Clamp(eulerAngles.y, -180.0f, 180.0f);

			return eulerAngles;
		}

		public static Vector2 GetClampedLookRotation(Vector2 lookRotation, float minPitch, float maxPitch)
		{
			lookRotation.x = Mathf.Clamp(lookRotation.x, minPitch, maxPitch);
			return lookRotation;
		}

		public static Vector2 GetClampedLookRotation(Vector2 lookRotation, Vector2 lookRotationDelta, float minPitch, float maxPitch)
		{
			return lookRotation + GetClampedLookRotationDelta(lookRotation, lookRotationDelta, minPitch, maxPitch);
		}

		public static Vector2 GetClampedLookRotationDelta(Vector2 lookRotation, Vector2 lookRotationDelta, float minPitch, float maxPitch)
		{
			Vector2 clampedlookRotationDelta = lookRotationDelta;
			lookRotationDelta.x = Mathf.Clamp(lookRotation.x + lookRotationDelta.x, minPitch, maxPitch) - lookRotation.x;
			return lookRotationDelta;
		}

		public static Vector3 EasyLerpDirection(Vector3 fromDirection, Vector3 toDirection, float time, float responsivity)
		{
			Vector3 interpolatedDirection = Vector3.Lerp(fromDirection, toDirection, time);

			float clampedResponsivity = Mathf.Clamp01(responsivity) * 2.0f;
			if (clampedResponsivity <= 1.0f)
			{
				float easedResponsivity = KCCMathUtility.EasyOut4(clampedResponsivity);
				return Vector3.Lerp(fromDirection, interpolatedDirection, easedResponsivity);
			}
			else
			{
				float easedResponsivity = KCCMathUtility.EasyIn4(clampedResponsivity - 1.0f);
				return Vector3.Lerp(interpolatedDirection, toDirection, easedResponsivity);
			}
		}
	}
}
