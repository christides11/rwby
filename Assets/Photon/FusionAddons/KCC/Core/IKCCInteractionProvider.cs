namespace Fusion.KCC
{
	/// <summary>
	/// Base interface for all KCC interaction providers.
	/// </summary>
	public interface IKCCInteractionProvider
	{
		/// <summary>
		/// Used to control start of the interaction with KCC. Executed on KCC input and state authority only.
		/// </summary>
		bool CanStartInteraction(KCC kcc, KCCData data);

		/// <summary>
		/// Used to control end of the interaction with KCC. Executed on KCC input and state authority only.
		/// All interactions are force stopped on despawn regardless of the return value.
		/// </summary>
		bool CanStopInteraction(KCC kcc, KCCData data);
	}
}
