namespace Fusion.KCC
{
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;

	public static partial class KCCIListExtensions
	{
		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int IndexOf<T>(this IList<T> list, T item)
		{
			return list.IndexOf(item);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddUnique<T>(this IList<T> list, T item)
		{
			if (list.Contains(item) == false)
			{
				list.Add(item);
			}
		}
	}
}
