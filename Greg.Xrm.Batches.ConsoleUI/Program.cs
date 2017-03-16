using Greg.Xrm.Batches.Core;
using log4net.Config;

namespace Greg.Xrm.Batches.ConsoleUI
{
	class Program
	{
		static void Main(string[] args)
		{
			XmlConfigurator.Configure();
			Bootstrapper.Boot(args).Wait();
		}
	}



	public class Flow1 : IFlow
	{
		public void Run(FlowContext context)
		{

		}
	}
	public class Flow2 : IFlow
	{
		public void Run(FlowContext context)
		{

		}
	}
}
