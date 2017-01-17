using CommandLine;
using CommandLine.Text;

namespace Greg.Xrm.Batches.Core
{
	internal class Options
	{
		[Option('f', "flow", Required = false, HelpText = "Full name of the flow to start")]
		public string FlowName { get; set; }

		[Option('c', "conn", Required = false, HelpText = "Connection string to use")]
		public string ConnectionString { get; set; }

		[Option('n', "connName", Required = false, HelpText = "Name of the connection string to use. A connection string with the given name must be configured in the app.config of the batch runner")]
		public string ConnectionStringName { get; set; }

		[Option('i', "interactive", Required = false, DefaultValue = false, HelpText = "Indicates whether the runner should show a prompt to the user")]
		public bool Interactive { get; set; }



		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}