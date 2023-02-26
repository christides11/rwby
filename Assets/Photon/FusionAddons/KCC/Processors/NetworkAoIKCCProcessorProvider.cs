namespace Fusion.KCC
{
	using UnityEngine;

	/// <summary>
	/// Default NetworkAoIKCCProcessor provider.
	/// </summary>
	[RequireComponent(typeof(NetworkObject))]
	public sealed class NetworkAoIKCCProcessorProvider : MonoBehaviour, IKCCProcessorProvider
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private NetworkAoIKCCProcessor _processor;

		// IKCCInteractionProvider INTERFACE

		bool IKCCInteractionProvider.CanStartInteraction(KCC kcc, KCCData data) => true;
		bool IKCCInteractionProvider.CanStopInteraction(KCC kcc, KCCData data)  => true;

		// IKCCProcessorProvider INTERFACE

		IKCCProcessor IKCCProcessorProvider.GetProcessor() => _processor;
	}
}
