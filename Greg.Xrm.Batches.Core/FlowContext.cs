using Microsoft.Xrm.Tooling.Connector;

namespace Greg.Xrm.Batches.Core
{
	public class FlowContext
	{
		internal FlowContext(string[] args, CrmServiceClient service)
		{
			this.Arguments = args;
			this.Service = service;
		}

		public string[] Arguments { get; }

		public CrmServiceClient Service { get; }
	}
}