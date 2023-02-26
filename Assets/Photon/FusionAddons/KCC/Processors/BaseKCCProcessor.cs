namespace Fusion.KCC
{
	using UnityEngine;

	/// <summary>
	/// Default processor implementation without support of [Networked] properties and runtime lookup. This processor type doesn't require NetworkObject and must be registered manually using KCC.AddLocalProcessor().
	/// Execution of methods is fully supported on 1) Prefabs, 2) Instances spawned with GameObject.Instantiate(), 3) Instances spawned with Runner.Spawn()
	/// </summary>
	public abstract partial class BaseKCCProcessor : MonoBehaviour, IKCCProcessor
	{
		// KCCProcessor INTERFACE

		/// <summary>
		/// Processors with higher priority are executed earlier.
		/// </summary>
		public virtual float Priority => default;

		/// <summary>
		/// Used to filter <c>KCCProcessor</c> stage method calls. Executed on KCC input and state authority only. Returns <c>EKCCStages.All</c> by default.
		/// EKCCStage which is not present in EKCCStages is always valid.
		/// </summary>
		public virtual EKCCStages GetValidStages(KCC kcc, KCCData data)
		{
			return EKCCStages.All;
		}

		/// <summary>
		/// Dedicated stage to set all input properties (ground angle, base position, gravity, ...). Executed on KCC input and state authority only.
		/// </summary>
		public virtual void SetInputProperties(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Dedicated stage to calculate KCCData.DynamicVelocity. Executed on KCC input and state authority only.
		/// </summary>
		public virtual void SetDynamicVelocity(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Dedicated stage to calculate KCCData.KinematicDirection. Executed on KCC input and state authority only.
		/// </summary>
		public virtual void SetKinematicDirection(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Dedicated stage to calculate KCCData.KinematicTangent. Executed on KCC input and state authority only.
		/// </summary>
		public virtual void SetKinematicTangent(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Dedicated stage to calculate KCCData.KinematicSpeed. Executed on KCC input and state authority only.
		/// </summary>
		public virtual void SetKinematicSpeed(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Dedicated stage to calculate KCCData.KinematicVelocity. Executed on KCC input and state authority only.
		/// </summary>
		public virtual void SetKinematicVelocity(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Dedicated stage to calculate properties after single physics query (for example kinematic velocity ground projection).
		/// This method can be called multiple times in a row if the KCC moves too fast (CCD is applied). Executed on KCC input and state authority only.
		/// </summary>
		public virtual void ProcessPhysicsQuery(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Called when a KCC starts interacting with the processor. Executed on KCC input and state authority only.
		/// </summary>
		public virtual void OnEnter(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Called when a KCC stops interacting with the processor. Executed on KCC input and state authority only.
		/// </summary>
		public virtual void OnExit(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Called when a KCC continues interacting with the processor. Executed on KCC input and state authority only.
		/// </summary>
		public virtual void OnStay(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Called when a KCC is interacting with the processor. Executed on KCC input/state authority with EKCCRenderBehavior.Interpolate and KCC proxy.
		/// </summary>
		public virtual void OnInterpolate(KCC kcc, KCCData data)
		{
		}

		/// <summary>
		/// Dedicated stage to process custom logic. Can be executed multiple times before or after KCC updates. Supports user data passed as parameter.
		/// Invoked from KCC.ProcessUserLogic(). Cannot be called when other stage is active. Executed on all (KCC input/state authority and KCC proxy).
		/// </summary>
		public virtual void ProcessUserLogic(KCC kcc, KCCData data, object userData)
		{
		}
	}
}
