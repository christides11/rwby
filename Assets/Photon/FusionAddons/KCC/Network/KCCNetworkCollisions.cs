namespace Fusion.KCC
{
	using System.Collections.Generic;

	public sealed unsafe class KCCNetworkCollisions : KCCNetworkProperty<KCCNetworkContext>
	{
		// PRIVATE MEMBERS

		private int _maxCount;

		// CONSTRUCTORS

		public KCCNetworkCollisions(KCCNetworkContext context, int maxCount) : base(context, 1 + maxCount * 4)
		{
			_maxCount = maxCount;
		}

		// KCCNetworkProperty INTERFACE

		public override void Read(int* ptr)
		{
			KCCData       data   = Context.Data;
			NetworkRunner runner = Context.KCC.Runner;

			data.Collisions.Clear(false);

			int  count        = *ptr;
			int* collisionPtr = ptr + 1;

			for (int i = 0; i < count; ++i)
			{
				KCCNetworkID networkID = KCCNetworkUtility.ReadNetworkID(collisionPtr);
				collisionPtr += 4;

				if (networkID.IsValid == true)
				{
					data.Collisions.Add(KCCNetworkID.GetNetworkObject(runner, networkID), networkID);
				}
			}
		}

		public override void Write(int* ptr)
		{
			KCCData data = Context.Data;

			int  collisionCount = 0;
			int* collisionPtr   = ptr + 1;

			List<KCCCollision> collisions = data.Collisions.All;
			for (int i = 0, count = collisions.Count; i < count; ++i)
			{
				KCCCollision collision = collisions[i];
				if (collision.NetworkID.IsValid == false)
					continue;

				KCCNetworkUtility.WriteNetworkID(collisionPtr, collision.NetworkID);
				collisionPtr += 4;

				++collisionCount;

				if (collisionCount >= _maxCount)
					break;
			}

			*ptr = collisionCount;
		}

		public override void Interpolate(InterpolationData interpolationData)
		{
		}
	}
}
