using System;
using System.Threading.Tasks;

namespace Greg.Xrm.Batches.Core.Interaction
{
	public interface IInteractionBroker : IDisposable
	{
		Task<bool> AskIfTheyWantToActuallyRunTheSpecifiedFlow(string flowName);
	}
}