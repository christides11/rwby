namespace Fusion.KCC
{
	using System.Runtime.CompilerServices;

	public static partial class KCCTypesExtensions
	{
		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has(this EKCCStages stages, EKCCStage stage)
		{
			return ((int)stages & (1 << (int)stage)) != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has(this EKCCFeatures features, EKCCFeature feature)
		{
			return ((int)features & (1 << (int)feature)) != 0;
		}
	}
}
