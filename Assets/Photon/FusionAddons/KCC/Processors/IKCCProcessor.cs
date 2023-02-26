namespace Fusion.KCC
{
	/// <summary>
	/// Base interface for all KCC processors.
	/// Execution of methods is fully supported on 1) Prefabs, 2) Instances spawned with GameObject.Instantiate(), 3) Instances spawned with Runner.Spawn()
	/// </summary>
	public partial interface IKCCProcessor
	{
		/// <summary>
		/// Processors with higher priority are executed earlier.
		/// </summary>
		float Priority { get; }

		/// <summary>
		/// Used to filter <c>IKCCProcessor</c> stage method calls. Executed on KCC input and state authority only.
		/// EKCCStage which is not present in EKCCStages is always valid.
		/// </summary>
		EKCCStages GetValidStages(KCC kcc, KCCData data);

		/// <summary>
		/// Dedicated stage to set all input properties (ground angle, base position, gravity, ...). Executed on KCC input and state authority only.
		/// </summary>
		void SetInputProperties(KCC kcc, KCCData data);

		/// <summary>
		/// Dedicated stage to calculate KCCData.DynamicVelocity. Executed on KCC input and state authority only.
		/// </summary>
		void SetDynamicVelocity(KCC kcc, KCCData data);

		/// <summary>
		/// Dedicated stage to calculate KCCData.KinematicDirection. Executed on KCC input and state authority only.
		/// </summary>
		void SetKinematicDirection(KCC kcc, KCCData data);

		/// <summary>
		/// Dedicated stage to calculate KCCData.KinematicTangent. Executed on KCC input and state authority only.
		/// </summary>
		void SetKinematicTangent(KCC kcc, KCCData data);

		/// <summary>
		/// Dedicated stage to calculate KCCData.KinematicSpeed. Executed on KCC input and state authority only.
		/// </summary>
		void SetKinematicSpeed(KCC kcc, KCCData data);

		/// <summary>
		/// Dedicated stage to calculate KCCData.KinematicVelocity. Executed on KCC input and state authority only.
		/// </summary>
		void SetKinematicVelocity(KCC kcc, KCCData data);

		/// <summary>
		/// Dedicated stage to calculate properties after single physics query (for example kinematic velocity ground projection).
		/// This method can be called multiple times in a row if the KCC moves too fast (CCD is applied). Executed on KCC input and state authority only.
		/// </summary>
		void ProcessPhysicsQuery(KCC kcc, KCCData data);

		/// <summary>
		/// Called when a KCC starts interacting with the processor. Executed on KCC input and state authority only.
		/// </summary>
		void OnEnter(KCC kcc, KCCData data);

		/// <summary>
		/// Called when a KCC stops interacting with the processor. Executed on KCC input and state authority only.
		/// </summary>
		void OnExit(KCC kcc, KCCData data);

		/// <summary>
		/// Called when a KCC continues interacting with the processor. Executed on KCC input and state authority only.
		/// </summary>
		void OnStay(KCC kcc, KCCData data);

		/// <summary>
		/// Called when a KCC is interacting with the processor. Executed on KCC input/state authority with EKCCRenderBehavior.Interpolate and KCC proxy.
		/// </summary>
		void OnInterpolate(KCC kcc, KCCData data);

		/// <summary>
		/// Dedicated stage to process custom logic. Can be executed multiple times before or after KCC updates. Supports user data passed as parameter.
		/// Invoked from KCC.ProcessUserLogic(). Cannot be called when other stage is active. Executed on all (input/state authority + proxy).
		/// </summary>
		void ProcessUserLogic(KCC kcc, KCCData data, object userData);
	}
}
