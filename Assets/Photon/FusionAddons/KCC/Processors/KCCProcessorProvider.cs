namespace Fusion.KCC
{
	using UnityEngine;

	/// <summary>
	/// Default KCCProcessor provider.
	/// </summary>
	[RequireComponent(typeof(NetworkObject))]
	public sealed class KCCProcessorProvider : MonoBehaviour, IKCCProcessorProvider
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private KCCProcessor _processor;

		// IKCCInteractionProvider INTERFACE

		bool IKCCInteractionProvider.CanStartInteraction(KCC kcc, KCCData data) => true;
		bool IKCCInteractionProvider.CanStopInteraction(KCC kcc, KCCData data)  => true;

		// IKCCProcessorProvider INTERFACE

		IKCCProcessor IKCCProcessorProvider.GetProcessor() => _processor;
	}
}
