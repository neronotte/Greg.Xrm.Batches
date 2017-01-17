using System.Configuration;

namespace Greg.Xrm.Batches.Core.Configuration
{
	public class FlowConfigurationElementCollection : ConfigurationElementCollection
	{
		public void Add(FlowConfigurationElement formConfigurationElement)
		{
			this.BaseAdd(formConfigurationElement);
		}

		protected override void BaseAdd(ConfigurationElement element)
		{
			base.BaseAdd(element, false);
		}

		public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.AddRemoveClearMap;

		protected override ConfigurationElement CreateNewElement()
		{
			return new FlowConfigurationElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((FlowConfigurationElement)element).Type;
		}

		public FlowConfigurationElement this[int index]
		{
			get
			{
				return (FlowConfigurationElement)this.BaseGet(index);
			}
			set
			{
				if (this.BaseGet(index) != null)
				{
					this.BaseRemoveAt(index);
				}
				this.BaseAdd(index, value);
			}
		}

		public new FlowConfigurationElement this[string name] => (FlowConfigurationElement)this.BaseGet(name);


		public void Remove(FlowConfigurationElement url)
		{
			if (this.BaseIndexOf(url) >= 0)
				this.BaseRemove(url.Type);
		}

		public void RemoveAt(int index)
		{
			this.BaseRemoveAt(index);
		}

		public void Remove(string name)
		{
			this.BaseRemove(name);
		}

		public void Clear()
		{
			this.BaseClear();
		}
	}
}