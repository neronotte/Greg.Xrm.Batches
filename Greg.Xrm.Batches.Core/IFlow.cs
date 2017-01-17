namespace Greg.Xrm.Batches.Core
{
	public interface IFlow
	{
		void Run(FlowContext context);
	}
}