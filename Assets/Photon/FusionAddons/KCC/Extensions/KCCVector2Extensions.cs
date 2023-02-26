namespace Fusion.KCC
{
	using System.Runtime.CompilerServices;
	using UnityEngine;

	public static partial class KCCVector2Extensions
	{
		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 X0Y(this Vector2 vector)
		{
			return new Vector3(vector.x, 0.0f, vector.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNaN(this Vector2 vector)
		{
			return float.IsNaN(vector.x) == true || float.IsNaN(vector.y) == true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsZero(this Vector2 vector)
		{
			return vector.x == 0.0f && vector.y == 0.0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAlmostZero(this Vector2 vector, float tolerance = 0.01f)
		{
			return vector.x < tolerance && vector.x > -tolerance && vector.y < tolerance && vector.y > -tolerance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEqual(this Vector2 vector, Vector2 other)
		{
			return vector.x == other.x && vector.y == other.y;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AlmostEquals(this Vector2 vectorA, Vector2 vectorB, float tolerance = 0.01f)
		{
			return IsAlmostZero(vectorA - vectorB, tolerance);
		}
	}
}
