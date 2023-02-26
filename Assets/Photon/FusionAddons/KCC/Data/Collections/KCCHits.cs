namespace Fusion.KCC
{
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Data structure representing single collider/trigger overlap (radius + extent). Read-only, managed entirely by <c>KCC</c>.
	/// </summary>
	public sealed partial class KCCHit
	{
		// PUBLIC MEMBERS

		/// <summary>Reference to collider/trigger component.</summary>
		public Collider Collider;

		/// <summary>Reference to collider transform component.</summary>
		public Transform Transform;

		/// <summary>
		/// Collision type, valid only for penetrating collisions.
		/// Non-penetrating collisions within (radius + extent) have ECollisionType.None.
		/// </summary>
		public ECollisionType CollisionType;

		// PUBLIC METHODS

		public void CopyFromOther(KCCHit other)
		{
			Collider      = other.Collider;
			Transform     = other.Transform;
			CollisionType = other.CollisionType;
		}

		public void Clear()
		{
			Collider      = default;
			Transform     = default;
			CollisionType = default;
		}
	}

	/// <summary>
	/// Collection dedicated to tracking all colliders/triggers the KCC collides with (radius + extent). Managed entirely by <c>KCC</c>.
	/// </summary>
	public sealed partial class KCCHits
	{
		// PUBLIC MEMBERS

		public readonly List<KCCHit> All = new List<KCCHit>();

		public int Count => All.Count;

		// PRIVATE MEMBERS

		private Stack<KCCHit> _pool = new Stack<KCCHit>();

		// PUBLIC METHODS

		public bool HasCollider(Collider collider)
		{
			return Find(collider, out int index) != null;
		}

		public KCCHit Add(KCCOverlapHit overlapHit)
		{
			KCCHit hit = GetFromPool();
			hit.Collider      = overlapHit.Collider;
			hit.Transform     = overlapHit.Transform;
			hit.CollisionType = overlapHit.CollisionType;

			All.Add(hit);

			return hit;
		}

		public void CopyFromOther(KCCHits other)
		{
			if (All.Count == other.All.Count)
			{
				for (int i = 0, count = All.Count; i < count; ++i)
				{
					All[i].CopyFromOther(other.All[i]);
				}
			}
			else
			{
				Clear(false);

				for (int i = 0, count = other.All.Count; i < count; ++i)
				{
					KCCHit hit = GetFromPool();
					hit.CopyFromOther(other.All[i]);
					All.Add(hit);
				}
			}
		}

		public void Clear(bool clearPool)
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				ReturnToPool(All[i]);
			}

			All.Clear();

			if (clearPool == true)
			{
				_pool.Clear();
			}
		}

		// PRIVATE METHODS

		private KCCHit Find(Collider collider, out int index)
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				KCCHit hit = All[i];
				if (object.ReferenceEquals(hit.Collider, collider) == true)
				{
					index = i;
					return hit;
				}
			}

			index = -1;
			return default;
		}

		private KCCHit GetFromPool()
		{
			return _pool.Count > 0 ? _pool.Pop() : new KCCHit();
		}

		private void ReturnToPool(KCCHit hit)
		{
			hit.Clear();
			_pool.Push(hit);
		}
	}
}
