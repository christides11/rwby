namespace Fusion.KCC
{
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Container for collision-based interactions. Collider and Processor need to be cached (accessed every frame).
	/// </summary>
	public sealed partial class KCCCollision : KCCInteraction<KCCCollision>
	{
		// PUBLIC MEMBERS

		public Collider      Collider;
		public IKCCProcessor Processor;

		// KCCInteraction<TInteraction> INTERFACE

		protected override void OnInitialize()
		{
			Collider  = NetworkObject.GetComponentNoAlloc<Collider>();
			Processor = Provider is IKCCProcessorProvider processorProvider ? processorProvider.GetProcessor() : null;
		}

		protected override void OnDeinitialize()
		{
			Collider  = null;
			Processor = null;
		}

		protected override void OnCopyFromOther(KCCCollision other)
		{
			Collider  = other.Collider;
			Processor = other.Processor;
		}
	}

	/// <summary>
	/// Collection dedicated to tracking of collision-based interactions with colliders/triggers and related processors. Managed entirely by <c>KCC</c> component.
	/// </summary>
	public sealed partial class KCCCollisions : KCCInteractions<KCCCollision>
	{
		// PUBLIC METHODS

		public bool HasCollider(Collider collider)
		{
			return Find(collider, out int index) != null;
		}

		public bool HasProcessor<T>() where T : class
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				if (All[i].Processor is T)
					return true;
			}

			return false;
		}

		public bool HasProcessor<T>(T processor) where T : Component, IKCCProcessor
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				if (object.ReferenceEquals(All[i].Processor, processor) == true)
					return true;
			}

			return false;
		}

		public T GetProcessor<T>() where T : class
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				if (All[i].Processor is T processor)
					return processor;
			}

			return default;
		}

		public void GetProcessors<T>(List<T> processors, bool clearList = true) where T : class
		{
			if (clearList == true)
			{
				processors.Clear();
			}

			for (int i = 0, count = All.Count; i < count; ++i)
			{
				if (All[i].Processor is T processor)
				{
					processors.Add(processor);
				}
			}
		}

		public KCCCollision Add(NetworkObject networkObject, IKCCInteractionProvider provider, Collider collider)
		{
			KCCCollision collision = AddInternal(networkObject, provider, false);
			collision.Collider  = collider;
			collision.Processor = provider is IKCCProcessorProvider processorProvider ? processorProvider.GetProcessor() : null;

			return collision;
		}

		// PRIVATE METHODS

		private KCCCollision Find(Collider collider, out int index)
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				KCCCollision collision = All[i];
				if (object.ReferenceEquals(collision.Collider, collider) == true)
				{
					index = i;
					return collision;
				}
			}

			index = -1;
			return default;
		}
	}
}
