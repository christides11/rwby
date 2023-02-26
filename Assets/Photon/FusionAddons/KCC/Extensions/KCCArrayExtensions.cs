namespace Fusion.KCC
{
	using System;
	using System.Runtime.CompilerServices;

	public static partial class KCCArrayExtensions
	{
		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Clear(this Array array)
		{
			Array.Clear(array, 0, array.Length);
		}
	}
}
