namespace Fusion.KCC
{
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Collection dedicated to tracking ignored colliders. Managed entirely by <c>KCC</c> component.
	/// </summary>
	public sealed class KCCIgnores
	{
		// PUBLIC MEMBERS

		public readonly List<KCCIgnore> All = new List<KCCIgnore>();

		public int Count => All.Count;

		// PRIVATE MEMBERS

		private Stack<KCCIgnore> _pool = new Stack<KCCIgnore>();

		// PUBLIC METHODS

		public bool HasCollider(Collider collider)
		{
			return Find(collider, out int index) != null;
		}

		public KCCIgnore Add(NetworkObject networkObject, Collider collider, bool checkExisting)
		{
			KCCIgnore ignore = checkExisting == true ? Find(collider, out int index) : null;
			if (ignore == null)
			{
				ignore = GetFromPool();

				ignore.NetworkID     = KCCNetworkID.GetNetworkID(networkObject);
				ignore.NetworkObject = networkObject;
				ignore.Collider      = collider;

				All.Add(ignore);
			}

			return ignore;
		}

		public void Add(NetworkObject networkObject, KCCNetworkID networkID)
		{
			if (networkObject == null)
				return;

			KCCIgnore ignore = GetFromPool();
			ignore.NetworkID     = networkID;
			ignore.NetworkObject = networkObject;
			ignore.Collider      = networkObject.GetComponentNoAlloc<Collider>();

			All.Add(ignore);
		}

		public bool Remove(Collider collider)
		{
			KCCIgnore ignore = Find(collider, out int index);
			if (ignore != null)
			{
				All.RemoveAt(index);
				ReturnToPool(ignore);
				return true;
			}

			return false;
		}

		public void CopyFromOther(KCCIgnores other)
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
					KCCIgnore ignore = GetFromPool();
					ignore.CopyFromOther(other.All[i]);
					All.Add(ignore);
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

		private KCCIgnore Find(Collider collider, out int index)
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				KCCIgnore ignore = All[i];
				if (object.ReferenceEquals(ignore.Collider, collider) == true)
				{
					index = i;
					return ignore;
				}
			}

			index = -1;
			return default;
		}

		private KCCIgnore GetFromPool()
		{
			return _pool.Count > 0 ? _pool.Pop() : new KCCIgnore();
		}

		private void ReturnToPool(KCCIgnore ignore)
		{
			ignore.Clear();
			_pool.Push(ignore);
		}
	}

	public sealed class KCCIgnore
	{
		public KCCNetworkID  NetworkID;
		public NetworkObject NetworkObject;
		public Collider      Collider;

		public void CopyFromOther(KCCIgnore other)
		{
			NetworkID     = other.NetworkID;
			NetworkObject = other.NetworkObject;
			Collider      = other.Collider;
		}

		public void Clear()
		{
			NetworkID     = default;
			NetworkObject = default;
			Collider      = default;
		}
	}
}
