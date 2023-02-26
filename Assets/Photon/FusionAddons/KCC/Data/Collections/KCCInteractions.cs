namespace Fusion.KCC
{
	using System.Collections.Generic;

	/// <summary>
	/// Base container for all interactions.
	/// </summary>
	public abstract partial class KCCInteraction<TInteraction> where TInteraction : KCCInteraction<TInteraction>, new()
	{
		// PUBLIC MEMBERS

		public KCCNetworkID            NetworkID;
		public NetworkObject           NetworkObject;
		public IKCCInteractionProvider Provider;

		// KCCInteraction<TInteraction> INTERFACE

		protected abstract void OnInitialize();
		protected abstract void OnDeinitialize();
		protected abstract void OnCopyFromOther(TInteraction other);

		// PUBLIC METHODS

		public void Initialize(KCCNetworkID networkID, NetworkObject networkObject, IKCCInteractionProvider provider, bool initializeDeep)
		{
			NetworkID     = networkID;
			NetworkObject = networkObject;
			Provider      = provider;

			if (initializeDeep == true)
			{
				OnInitialize();
			}
		}

		public void Deinitialize()
		{
			OnDeinitialize();

			NetworkID     = default;
			NetworkObject = default;
			Provider      = default;
		}

		public void CopyFromOther(TInteraction other)
		{
			NetworkID     = other.NetworkID;
			NetworkObject = other.NetworkObject;
			Provider      = other.Provider;

			OnCopyFromOther(other);
		}
	}

	/// <summary>
	/// Base collection for tracking all interactions and their providers.
	/// </summary>
	public abstract partial class KCCInteractions<TInteraction> where TInteraction : KCCInteraction<TInteraction>, new ()
	{
		// PUBLIC MEMBERS

		public readonly List<TInteraction> All = new List<TInteraction>();

		public int Count => All.Count;

		// PRIVATE MEMBERS

		private Stack<TInteraction> _pool = new Stack<TInteraction>();

		// PUBLIC METHODS

		public bool HasProvider<T>() where T : class
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				if (All[i].Provider is T)
					return true;
			}

			return false;
		}

		public bool HasProvider(IKCCInteractionProvider provider)
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				if (object.ReferenceEquals(All[i].Provider, provider) == true)
					return true;
			}

			return false;
		}

		public T GetProvider<T>() where T : class
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				if (All[i].Provider is T provider)
					return provider;
			}

			return default;
		}

		public void GetProviders<T>(List<T> providers, bool clearList = true) where T : class
		{
			if (clearList == true)
			{
				providers.Clear();
			}

			for (int i = 0, count = All.Count; i < count; ++i)
			{
				if (All[i].Provider is T provider)
				{
					providers.Add(provider);
				}
			}
		}

		public TInteraction Find(IKCCInteractionProvider provider)
		{
			return Find(provider, out int index);
		}

		public TInteraction Add(NetworkObject networkObject, IKCCInteractionProvider provider)
		{
			return AddInternal(networkObject, provider, true);
		}

		public void Add(NetworkObject networkObject, KCCNetworkID networkID)
		{
			if (networkObject == null)
				return;

			IKCCInteractionProvider provider = networkObject.GetComponentNoAlloc<IKCCInteractionProvider>();

			TInteraction interaction = GetFromPool();
			interaction.Initialize(networkID, networkObject, provider, true);

			All.Add(interaction);
		}

		public bool Remove(TInteraction interaction)
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				if (All[i] == interaction)
				{
					All.RemoveAt(i);
					ReturnToPool(interaction);
					return true;
				}
			}

			return false;
		}

		public void CopyFromOther<T>(T other) where T : KCCInteractions<TInteraction>
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
					TInteraction interaction = GetFromPool();
					interaction.CopyFromOther(other.All[i]);
					All.Add(interaction);
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

		// PROTECTED METHODS

		protected TInteraction AddInternal(NetworkObject networkObject, IKCCInteractionProvider provider, bool initializeDeep)
		{
			TInteraction interaction = GetFromPool();
			interaction.Initialize(KCCNetworkID.GetNetworkID(networkObject), networkObject, provider, initializeDeep);

			All.Add(interaction);

			return interaction;
		}

		protected TInteraction Find(IKCCInteractionProvider provider, out int index)
		{
			for (int i = 0, count = All.Count; i < count; ++i)
			{
				TInteraction interaction = All[i];
				if (object.ReferenceEquals(interaction.Provider, provider) == true)
				{
					index = i;
					return interaction;
				}
			}

			index = -1;
			return default;
		}

		// PRIVATE METHODS

		private TInteraction GetFromPool()
		{
			return _pool.Count > 0 ? _pool.Pop() : new TInteraction();
		}

		private void ReturnToPool(TInteraction interaction)
		{
			interaction.Deinitialize();
			_pool.Push(interaction);
		}
	}
}
