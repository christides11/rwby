namespace Fusion.KCC
{
	using UnityEngine;

	public sealed class KCCRaycastInfo
	{
		// PUBLIC MEMBERS

		public Vector3                 Origin;
		public Vector3                 Direction;
		public float                   MaxDistance;
		public float                   Radius;
		public LayerMask               LayerMask;
		public QueryTriggerInteraction TriggerInteraction;
		public RaycastHit[]            Hits;
		public int                     HitCount;

		// CONSTRUCTORS

		public KCCRaycastInfo(int maxHits)
		{
			Hits = new RaycastHit[maxHits];
		}

		// PUBLIC METHODS

		public void AddHit(RaycastHit hit)
		{
			if (HitCount == Hits.Length)
				return;

			Hits[HitCount] = hit;
			++HitCount;
		}

		public void Reset(bool clearArrays)
		{
			Origin             = default;
			Direction          = default;
			MaxDistance        = default;
			Radius             = default;
			LayerMask          = default;
			TriggerInteraction = QueryTriggerInteraction.Collide;
			HitCount           = default;

			if (clearArrays == true)
			{
				Hits.Clear();
			}
		}
	}
}
