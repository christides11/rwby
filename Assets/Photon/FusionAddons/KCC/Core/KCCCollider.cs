namespace Fusion.KCC
{
	using UnityEngine;

	/// <summary>
	/// Custom wrapper for fast property checks and synchronization. Don't touch.
	/// </summary>
	public sealed class KCCCollider
	{
		// PUBLIC MEMBERS

		public GameObject      GameObject;
		public Transform       Transform;
		public CapsuleCollider Collider;
		public bool            IsSpawned;
		public bool            IsTrigger;
		public float           Radius;
		public float           Height;
		public int             Layer;

		// PUBLIC METHODS

		public void Update(Transform parent, KCCSettings settings)
		{
			if (IsSpawned == false)
			{
				IsTrigger = settings.IsTrigger;
				Radius    = settings.Radius;
				Height    = settings.Height;
				Layer     = settings.ColliderLayer;

				GameObject = new GameObject("KCCCollider");
				GameObject.layer = settings.ColliderLayer;

				Transform = GameObject.transform;
				Transform.SetParent(parent, false);
				Transform.localPosition = Vector3.zero;
				Transform.localRotation = Quaternion.identity;
				Transform.localScale    = Vector3.one;

				Collider = GameObject.AddComponent<CapsuleCollider>();
				Collider.direction = 1;
				Collider.isTrigger = settings.IsTrigger;
				Collider.radius    = settings.Radius;
				Collider.height    = settings.Height;
				Collider.center    = new Vector3(0.0f, settings.Height * 0.5f, 0.0f);

				IsSpawned = true;
			}

			if (IsTrigger != settings.IsTrigger)
			{
				IsTrigger = settings.IsTrigger;
				Collider.isTrigger = settings.IsTrigger;
			}

			if (Radius != settings.Radius)
			{
				Radius = settings.Radius;
				Collider.radius = settings.Radius;
			}

			if (Height != settings.Height)
			{
				Height = settings.Height;
				Collider.height = settings.Height;
				Collider.center = new Vector3(0.0f, settings.Height * 0.5f, 0.0f);
			}

			if (Layer != settings.ColliderLayer)
			{
				Layer = settings.ColliderLayer;
				GameObject.layer = settings.ColliderLayer;
			}
		}

		public void Destroy()
		{
			if (IsSpawned == false)
				return;

			if (Collider != null)
			{
				Collider.enabled = false;
			}

			GameObject.Destroy(GameObject);

			GameObject = default;
			Transform  = default;
			Collider   = default;
			IsSpawned  = default;
			IsTrigger  = default;
			Radius     = default;
			Height     = default;
			Layer      = default;
		}
	}
}
