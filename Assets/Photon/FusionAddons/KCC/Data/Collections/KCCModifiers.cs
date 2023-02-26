namespace Fusion.KCC
{
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Container for manually registered modifiers. Processor needs to be cached (accessed every frame).
	/// </summary>
	public sealed partial class KCCModifier : KCCInteraction<KCCModifier>
	{
		// PUBLIC MEMBERS

		public IKCCProcessor Processor;

		// KCCInteraction<TInteraction> INTERFACE

		protected override void OnInitialize()
		{
			Processor = Provider is IKCCProcessorProvider processorProvider ? processorProvider.GetProcessor() : null;
		}

		protected override void OnDeinitialize()
		{
			Processor = null;
		}

		protected override void OnCopyFromOther(KCCModifier other)
		{
			Processor = other.Processor;
		}
	}

	/// <summary>
	/// Collection dedicated to tracking of manually registered modifiers and their processors. Managed entirely by <c>KCC</c> component.
	/// </summary>
	public sealed partial class KCCModifiers : KCCInteractions<KCCModifier>
	{
		// PUBLIC METHODS

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
	}
}
