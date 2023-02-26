namespace Fusion.KCC
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Custom updater for KCCs with <c>EKCCDriver.Unity</c> driver. Only for internal use, managed entirely by <c>KCC</c> component at runtime.
	/// </summary>
	public sealed class KCCUpdater : MonoBehaviour
	{
		// PRIVATE MEMBERS

		private Action _fixedUpdate;
		private Action _update;

		// PUBLIC METHODS

		public void Initialize(Action fixedUpdate, Action update)
		{
			_fixedUpdate = fixedUpdate;
			_update      = update;

			enabled = true;
		}

		public void Deinitialize()
		{
			enabled = false;

			_fixedUpdate = null;
			_update      = null;
		}

		// PRIVATE METHODS

		private void FixedUpdate()
		{
			if (_fixedUpdate != null)
			{
				_fixedUpdate();
			}
		}

		private void Update()
		{
			if (_update != null)
			{
				_update();
			}
		}
	}
}
