using System.Configuration;

namespace Greg.Xrm.Batches.Core.Configuration
{
	public class FlowConfigurationElement : ConfigurationElement
	{
		[ConfigurationProperty("type", IsRequired = true)]
		public string Type
		{
			get { return (string)this["type"]; }
			set { this["type"] = value; }
		}

		[ConfigurationProperty("shouldrun", IsRequired = false, DefaultValue = false)]
		public bool ShouldRun
		{
			get { return (bool)this["shouldrun"]; }
			set { this["shouldrun"] = value; }
		}
	}
}