namespace Fusion.KCC
{
	using System.Runtime.CompilerServices;

	public static partial class KCCFloatExtensions
	{
		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNaN(this float value)
		{
			return float.IsNaN(value) == true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAlmostZero(this float value, float tolerance = 0.01f)
		{
			return value < tolerance && value > -tolerance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AlmostEquals(this float valueA, float valueB, float tolerance = 0.01f)
		{
			return IsAlmostZero(valueA - valueB, tolerance);
		}
	}
}
