using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Avanade.Rina.Batches.Core.Common;
using Common.Logging;
using Greg.Xrm.Batches.Core.Common;
using Greg.Xrm.Batches.Core.Configuration;
using Greg.Xrm.Batches.Core.Interaction;
using Greg.Xrm.Batches.Core.IoC;
using Microsoft.Practices.Unity;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace Greg.Xrm.Batches.Core
{
	public class Bootstrapper
	{
		private readonly ILog log = LogManager.GetLogger<Bootstrapper>();


		public static async Task Boot(IInteractionBroker interactionBroker, params string[] args)
		{
			var bootstrapper = new Bootstrapper(interactionBroker);
			await bootstrapper.Initialize(args ?? new string[0]);
		}

		public static async Task Boot(params string[] args)
		{
			var bootstrapper = new Bootstrapper(new ConsoleInteractionBroker());
			await bootstrapper.Initialize(args ?? new string[0]);
		}


		private IInteractionBroker interactionBroker;

		private Bootstrapper(IInteractionBroker interactionBroker)
		{
			if (interactionBroker == null)
				throw new ArgumentNullException(nameof(interactionBroker));

			this.interactionBroker = interactionBroker;
		}





		public async Task Initialize(string[] args)
		{
			var options = new Options();
			var isValid = CommandLine.Parser.Default.ParseArguments(args, options);
			if (!isValid)
			{
				return;
			}

			if (!options.Interactive)
			{
				this.interactionBroker.Dispose();
				this.interactionBroker = new NoInteractionBroker();
			}


			// creating container
			var container = new UnityContainer();
			try
			{
				var containerConfiguratorType = typeof(IUnityConfigurator);
				var configuratorList = AllClasses.FromAssembliesInBasePath()
					.Where(x => containerConfiguratorType.IsAssignableFrom(x))
					.Where(x => !x.IsAbstract)
					.Where(x => x.GetConstructor(new Type[0]) != null)
					.Select(x => (IUnityConfigurator)container.Resolve(x));

				foreach (var configurator in configuratorList)
				{
					configurator.Configure(container);
				}
			}
			catch (Exception ex)
			{
				log.Error($"Error while configuring unity container: {ex.Message}", ex);
				return;
			}


			



			// loading services
			IEnumerable<IFlow> flowsToRun;
			try
			{
				flowsToRun = GetFlowsToRun(container, options);
			}
			catch (Exception ex)
			{
				log.Error($"Error while identifying flows to run: {ex.Message}", ex);
				return;
			}



			

			// creating service client
			CrmServiceClient client;
			try
			{
				var factory = new CrmServiceClientFactory();
				client = factory.CreateCrmServiceClient(options);
			}
			catch (ConfigurationErrorsException ex)
			{
				log.Error(ex.Message);
				return;
			}
			



			



			var context = new FlowContext(args, client);

			foreach (var flow in flowsToRun)
			{
				var shouldRun = await this.interactionBroker.AskIfTheyWantToActuallyRunTheSpecifiedFlow(flow.GetType().FullName);
				if (!shouldRun)
					continue;

				using (this.log.Track($"Running flow <{flow.GetType().FullName}>"))
				{
					try
					{
						flow.Run(context);
					}
					catch (FaultException<OrganizationServiceFault> ex)
					{
						this.log.Error($"Unhandled error in flow <{flow.GetType().FullName}>: {ex.Message}{Environment.NewLine}{ex.Detail?.TraceText}", ex);
					}
					catch (Exception ex)
					{
						this.log.Error($"Unhandled error in flow <{flow.GetType().FullName}>: {ex.Message}", ex);
					}
				}
			}
		}



		private static IEnumerable<IFlow> GetFlowsToRun(IUnityContainer container, Options options)
		{
			var flowType = typeof(IFlow);
			var query = AllClasses.FromAssembliesInBasePath()
				.Where(x => flowType.IsAssignableFrom(x))
				.Where(x => !x.IsAbstract);

			if (!string.IsNullOrWhiteSpace(options.FlowName))
			{
				query = query.Where( x =>
							x.FullName.Equals(options.FlowName, StringComparison.OrdinalIgnoreCase) ||
							(x.AssemblyQualifiedName?.Equals(options.FlowName, StringComparison.OrdinalIgnoreCase) ?? false));
			}
			else
			{
				var section = (BatchesConfigurationSection)ConfigurationManager.GetSection("Avanade.Xrm.Batches");

				var flowNames = section.Flows
					.Cast<FlowConfigurationElement>()
					.Where(bct => bct.ShouldRun)
					.Select(x => x.Type)
					.ToList();


				query = query.Where(x => flowNames.Contains(x.FullName) || flowNames.Contains(x.AssemblyQualifiedName));
			}



			var flowList = query.Select(x => (IFlow) container.Resolve(x));
			return flowList;
		}
	}
}
