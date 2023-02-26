namespace Fusion.KCC
{
	using UnityEngine;

	/// <summary>
	/// Data structure used to store/restore and process data from/to KCCData before/after KCC moves in fixed/render update.
	/// Simply zeroing values after KCC update isn't enough because values can be modified during update by processors which could result in loss.
	/// By default jump impulse and external forces won't propagate to next frame.
	/// </summary>
	public sealed partial class KCCTransientData
	{
		// PUBLIC MEMBERS

		public Vector3 JumpImpulse;
		public Vector3 ExternalVelocity;
		public Vector3 ExternalAcceleration;
		public Vector3 ExternalImpulse;
		public Vector3 ExternalForce;

		// PUBLIC METHODS

		/// <summary>
		/// Called before the KCC moves in update loop.
		/// </summary>
		public void Store(KCC kcc, KCCData data)
		{
			JumpImpulse          = data.JumpImpulse;
			ExternalVelocity     = data.ExternalVelocity;
			ExternalAcceleration = data.ExternalAcceleration;
			ExternalImpulse      = data.ExternalImpulse;
			ExternalForce        = data.ExternalForce;

			StoreUserData(kcc, data);
		}

		/// <summary>
		/// Called after the KCC moves in update loop.
		/// </summary>
		public void Restore(KCC kcc, KCCData data)
		{
			data.JumpImpulse          -= JumpImpulse;
			data.ExternalVelocity     -= ExternalVelocity;
			data.ExternalAcceleration -= ExternalAcceleration;
			data.ExternalImpulse      -= ExternalImpulse;
			data.ExternalForce        -= ExternalForce;

			RestoreUserData(kcc, data);
		}

		/// <summary>
		/// Optional reset, can be invoked from user code if desired.
		/// </summary>
		public void Clear()
		{
			JumpImpulse          = default;
			ExternalVelocity     = default;
			ExternalAcceleration = default;
			ExternalImpulse      = default;
			ExternalForce        = default;

			ClearUserData();
		}

		// PARTIAL METHODS

		partial void StoreUserData(KCC kcc, KCCData data);
		partial void RestoreUserData(KCC kcc, KCCData data);
		partial void ClearUserData();
	}
}
