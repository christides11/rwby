namespace Fusion.KCC
{
	using UnityEngine;

	public sealed class KCCOverlapInfo
	{
		// PUBLIC MEMBERS

		public Vector3                 Position;
		public float                   Radius;
		public float                   Height;
		public float                   Extent;
		public LayerMask               LayerMask;
		public QueryTriggerInteraction TriggerInteraction;
		public KCCOverlapHit[]         AllHits;
		public int                     AllHitCount;
		public KCCOverlapHit[]         ColliderHits;
		public int                     ColliderHitCount;
		public KCCOverlapHit[]         TriggerHits;
		public int                     TriggerHitCount;

		// CONSTRUCTORS

		public KCCOverlapInfo(int maxHits)
		{
			AllHits      = new KCCOverlapHit[maxHits];
			TriggerHits  = new KCCOverlapHit[maxHits];
			ColliderHits = new KCCOverlapHit[maxHits];

			for (int i = 0; i < maxHits; ++i)
			{
				AllHits[i] = new KCCOverlapHit();
			}
		}

		// PUBLIC METHODS

		public void AddHit(Collider collider)
		{
			if (AllHitCount == AllHits.Length)
				return;

			KCCOverlapHit hit = AllHits[AllHitCount];
			if (hit.Set(collider) == true)
			{
				++AllHitCount;

				if (hit.IsTrigger == true)
				{
					TriggerHits[TriggerHitCount] = hit;
					++TriggerHitCount;
				}
				else
				{
					ColliderHits[ColliderHitCount] = hit;
					++ColliderHitCount;
				}
			}
		}

		public void ToggleConvexMeshColliders(bool convex)
		{
			KCCOverlapHit hit;

			for (int i = 0; i < ColliderHitCount; ++i)
			{
				hit = ColliderHits[i];

				if (hit.Type == EColliderType.Mesh && hit.IsConvertible == true)
				{
					((MeshCollider)hit.Collider).convex = convex;
				}
			}
		}

		public bool AllWithinExtent()
		{
			KCCOverlapHit[] hits = AllHits;
			for (int i = 0, count = AllHitCount; i < count; ++i)
			{
				if (AllHits[i].IsWithinExtent == false)
					return false;
			}

			return true;
		}

		public void Reset(bool deep)
		{
			Position           = default;
			Radius             = default;
			Height             = default;
			Extent             = default;
			LayerMask          = default;
			TriggerInteraction = QueryTriggerInteraction.Collide;
			AllHitCount        = default;
			TriggerHitCount    = default;
			ColliderHitCount   = default;

			if (deep == true)
			{
				for (int i = 0, count = AllHits.Length; i < count; ++i)
				{
					AllHits[i].Reset();
				}
			}
		}

		public void CopyFromOther(KCCOverlapInfo other)
		{
			Position           = other.Position;
			Radius             = other.Radius;
			Height             = other.Height;
			Extent             = other.Extent;
			LayerMask          = other.LayerMask;
			TriggerInteraction = other.TriggerInteraction;
			AllHitCount        = other.AllHitCount;
			TriggerHitCount    = default;
			ColliderHitCount   = default;

			KCCOverlapHit hit;

			for (int i = 0; i < AllHitCount; ++i)
			{
				hit = AllHits[i];

				hit.CopyFromOther(other.AllHits[i]);

				if (hit.IsTrigger == true)
				{
					TriggerHits[TriggerHitCount] = hit;
					++TriggerHitCount;
				}
				else
				{
					ColliderHits[ColliderHitCount] = hit;
					++ColliderHitCount;
				}
			}
		}
	}
}
