using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace rwby
{
	/// <summary>
	/// Pool of all free instances of a single type of NetworkObject's
	/// </summary>
	public class FusionObjectPool
	{
		private List<NetworkObject> _free = new List<NetworkObject>();

		public NetworkObject GetFromPool(Vector3 p, Quaternion q, Transform parent = null)
		{
			NetworkObject newt = null;

			while (_free.Count > 0 && newt == null)
			{
				var t = _free[0];
				if (t) // In case a recycled object was destroyed
				{
					Transform xform = t.transform;
					xform.SetParent(parent, false);
					xform.position = p;
					xform.rotation = q;
					newt = t;
				}
				else
				{
					Debug.LogWarning("Recycled object was destroyed - not re-using!");
				}

				_free.RemoveAt(0);
			}

			return newt;
		}

		public void Clear()
		{
			foreach (var pooled in _free)
			{
				if (pooled)
				{
					Debug.Log($"Destroying pooled object: {pooled.gameObject.name}");
					Object.Destroy(pooled.gameObject);
				}
			}

			_free = new List<NetworkObject>();
		}

		public void ReturnToPool(NetworkObject no)
		{
			_free.Add(no);
		}
	}
}