using System;
using System.Threading.Tasks;

namespace Greg.Xrm.Batches.Core.Interaction
{
	public class ConsoleInteractionBroker : IInteractionBroker
	{
		public void Dispose()
		{
		}


		public Task<bool> AskIfTheyWantToActuallyRunTheSpecifiedFlow(string flowName)
		{
			string response;

			do
			{
				Console.WriteLine($"> Do you really want to run flow <{flowName}>? [Y/N]");
				Console.Write("> ");
				response = Console.ReadLine();

			} while (!"Y".Equals(response, StringComparison.OrdinalIgnoreCase) && !"N".Equals(response, StringComparison.OrdinalIgnoreCase));

			return Task.FromResult(response == "Y");
		}
	}
}