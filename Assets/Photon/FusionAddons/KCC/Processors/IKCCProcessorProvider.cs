namespace Fusion.KCC
{
	/// <summary>
	/// Base interface for all processor providers. By default KCC tracks interaction providers.
	/// This allows to reference processors indirectly, providing support for sharing/reuse of processor and defer processor selection based on provider current state.
	/// </summary>
	public interface IKCCProcessorProvider : IKCCInteractionProvider
	{
		IKCCProcessor GetProcessor();
	}
}
