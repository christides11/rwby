namespace Fusion.KCC
{
	using System;
	using UnityEngine;

	public sealed class KCCOverlapHit
	{
		// PUBLIC MEMBERS

		public EColliderType  Type;
		public Collider       Collider;
		public Transform      Transform;
		public bool           IsConvex;
		public bool           IsTrigger;
		public bool           IsPrimitive;
		public bool           IsConvertible;
		public bool           HasPenetration;
		public bool           IsWithinExtent;
		public ECollisionType CollisionType;

		// PRIVATE MEMBERS

		private static readonly Type SphereColliderType  = typeof(SphereCollider);
		private static readonly Type CapsuleColliderType = typeof(CapsuleCollider);
		private static readonly Type BoxColliderType     = typeof(BoxCollider);
		private static readonly Type MeshColliderType    = typeof(MeshCollider);
#if !KCC_DISABLE_TERRAIN
		private static readonly Type TerrainColliderType = typeof(TerrainCollider);
#endif

		// PUBLIC METHODS

		public bool IsValid() => Type != EColliderType.None;

		public bool Set(Collider collider)
		{
			Type colliderType = collider.GetType();

			if (colliderType == BoxColliderType)
			{
				Type          = EColliderType.Box;
				IsConvex      = true;
				IsPrimitive   = true;
				IsConvertible = false;
			}
			else if (colliderType == MeshColliderType)
			{
				MeshCollider meshCollider = (MeshCollider)collider;

				Type          = EColliderType.Mesh;
				IsConvex      = meshCollider.convex;
				IsPrimitive   = false;
				IsConvertible = false;

				if (IsConvex == true)
				{
					Mesh mesh = meshCollider.sharedMesh;
					IsConvertible = mesh != null && mesh.isReadable == true;
				}
			}
#if !KCC_DISABLE_TERRAIN
			else if (colliderType == TerrainColliderType)
			{
				Type          = EColliderType.Terrain;
				IsConvex      = false;
				IsPrimitive   = false;
				IsConvertible = false;
			}
#endif
			else if (colliderType == SphereColliderType)
			{
				Type          = EColliderType.Sphere;
				IsConvex      = true;
				IsPrimitive   = true;
				IsConvertible = false;
			}
			else if (colliderType == CapsuleColliderType)
			{
				Type          = EColliderType.Capsule;
				IsConvex      = true;
				IsPrimitive   = true;
				IsConvertible = false;
			}
			else
			{
				return false;
			}

			Collider       = collider;
			Transform      = collider.transform;
			IsTrigger      = collider.isTrigger;
			HasPenetration = default;
			IsWithinExtent = default;
			CollisionType  = default;

			return true;
		}

		public void Reset()
		{
			Type      = EColliderType.None;
			Collider  = default;
			Transform = default;
		}

		public void CopyFromOther(KCCOverlapHit other)
		{
			Type           = other.Type;
			Collider       = other.Collider;
			Transform      = other.Transform;
			IsConvex       = other.IsConvex;
			IsTrigger      = other.IsTrigger;
			IsPrimitive    = other.IsPrimitive;
			IsConvertible  = other.IsConvertible;
			HasPenetration = other.HasPenetration;
			IsWithinExtent = other.IsWithinExtent;
			CollisionType  = other.CollisionType;
		}
	}
}
