namespace Fusion.KCC
{
	using System.Runtime.CompilerServices;
	using UnityEngine;

	public static partial class KCCComponentExtensions
	{
		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetComponentNoAlloc<T>(this Component component) where T : class
		{
#if UNITY_EDITOR
			return GameObjectExtensions<T>.GetComponentNoAlloc(component.gameObject);
#else
			return component.GetComponent<T>();
#endif
		}
	}
}
