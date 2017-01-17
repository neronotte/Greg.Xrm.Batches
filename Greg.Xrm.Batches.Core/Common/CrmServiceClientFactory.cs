using System.Configuration;
using Microsoft.Xrm.Tooling.Connector;

namespace Greg.Xrm.Batches.Core.Common
{
	internal class CrmServiceClientFactory
	{
		public CrmServiceClient CreateCrmServiceClient(Options options)
		{
			if (string.IsNullOrWhiteSpace(options.ConnectionString) && string.IsNullOrWhiteSpace(options.ConnectionStringName))
			{
				return CreateCrmServiceClientFromAppConfig();
			}
			if (!string.IsNullOrWhiteSpace(options.ConnectionString))
			{
				return CreateCrmServiceClientFromConnectionString(options.ConnectionString);
			}
			if (!string.IsNullOrWhiteSpace(options.ConnectionStringName))
			{
				return CreateCrmServiceClientFromConnectionStringName(options.ConnectionString);
			}

			throw new ConfigurationErrorsException("Connection string not specified!");
		}

		private static CrmServiceClient CreateCrmServiceClientFromConnectionString(string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
			{
				throw new ConfigurationErrorsException($"Connection string <{connectionString}> is empty!");
			}

			return new CrmServiceClient(connectionString);
		}

		private static CrmServiceClient CreateCrmServiceClientFromConnectionStringName(string connectionStringName)
		{
			var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
			if (connectionString == null)
			{
				throw new ConfigurationErrorsException($"Unable to find connection string <{connectionStringName}>");
			}
			if (string.IsNullOrWhiteSpace(connectionString.ConnectionString))
			{
				throw new ConfigurationErrorsException($"Connection string <{connectionStringName}> is empty!");
			}

			return new CrmServiceClient(connectionString.ConnectionString);
		}

		private static CrmServiceClient CreateCrmServiceClientFromAppConfig()
		{
			var connectionStringName = ConfigurationManager.AppSettings["conn"];
			if (string.IsNullOrWhiteSpace(connectionStringName))
			{
				throw new ConfigurationErrorsException("Connection string not specified");
			}

			var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
			if (connectionString == null)
			{
				throw new ConfigurationErrorsException($"Unable to find connection string <{connectionStringName}>");
			}
			if (string.IsNullOrWhiteSpace(connectionString.ConnectionString))
			{
				throw new ConfigurationErrorsException($"Connection string <{connectionStringName}> is empty!");
			}

			return new CrmServiceClient(connectionString.ConnectionString);
		}
	}
}