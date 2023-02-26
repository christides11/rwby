namespace Fusion.KCC
{
	using UnityEngine;

	/// <summary>
	/// Default processor implementation without support of [Networked] properties. Supports runtime lookup and synchronization.
	/// Execution of methods is fully supported on 1) Prefabs, 2) Instances spawned with GameObject.Instantiate(), 3) Instances spawned with Runner.Spawn()
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkObject))]
	public abstract partial class KCCProcessor : BaseKCCProcessor, IKCCProcessorProvider
	{
		// IKCCInteractionProvider INTERFACE

		/// <summary>
		/// Used to control start of the interaction with KCC. Executed on KCC input and state authority only.
		/// </summary>
		public virtual bool CanStartInteraction(KCC kcc, KCCData data) => true;

		/// <summary>
		/// Used to control end of the interaction with KCC. Executed on KCC input and state authority only.
		/// All interactions are force stopped on despawn regardless of the return value.
		/// </summary>
		public virtual bool CanStopInteraction(KCC kcc, KCCData data) => true;

		// IKCCProcessorProvider INTERFACE

		IKCCProcessor IKCCProcessorProvider.GetProcessor() => this;
	}
}
