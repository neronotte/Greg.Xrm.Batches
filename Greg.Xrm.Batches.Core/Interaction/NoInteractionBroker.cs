using System.Threading.Tasks;

namespace Greg.Xrm.Batches.Core.Interaction
{
	internal class NoInteractionBroker : IInteractionBroker
	{
		public void Dispose()
		{
		}

		public Task<bool> AskIfTheyWantToActuallyRunTheSpecifiedFlow(string flowName)
		{
			return Task.FromResult(true);
		}
	}
}