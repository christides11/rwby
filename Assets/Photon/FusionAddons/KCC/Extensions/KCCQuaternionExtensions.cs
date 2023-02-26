namespace Fusion.KCC
{
	using System.Runtime.CompilerServices;
	using UnityEngine;

	public static partial class KCCQuaternionExtensions
	{
		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNaN(this Quaternion quaternion)
		{
			return float.IsNaN(quaternion.x) == true || float.IsNaN(quaternion.y) == true || float.IsNaN(quaternion.z) == true || float.IsNaN(quaternion.w) == true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsZero(this Quaternion quaternion)
		{
			return quaternion.x == 0.0f && quaternion.y == 0.0f && quaternion.z == 0.0f && quaternion.w == 0.0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEqual(this Quaternion quaternion, Quaternion other)
		{
			return quaternion.x == other.x && quaternion.y == other.y && quaternion.z == other.z && quaternion.w == other.w;
		}
	}
}
