using System.Configuration;

namespace Greg.Xrm.Batches.Core.Configuration
{
	public class BatchesConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("flows", IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(FlowConfigurationElementCollection), AddItemName = "add",
			ClearItemsName = "clear",
			RemoveItemName = "remove")]
		public FlowConfigurationElementCollection Flows => (FlowConfigurationElementCollection)base["flows"];
	}
}
